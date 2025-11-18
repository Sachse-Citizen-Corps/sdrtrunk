using System.Numerics;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using Microsoft.Extensions.Logging;
using SDRTrunk.DSP.Buffer;
using SDRTrunk.Models;
using SDRTrunk.Tuners.Base;
using SDRTrunk.Tuners.Exceptions;

namespace SDRTrunk.Tuners.RtlSdr;

/// <summary>
/// RTL-SDR tuner implementation using LibUsbDotNet
/// Supports RTL2832U-based DVB-T dongles
/// </summary>
public class RtlSdrTuner : BaseTuner
{
    private UsbDevice? _usbDevice;
    private UsbEndpointReader? _bulkReader;
    private readonly int _deviceIndex;
    private string _serialNumber = string.Empty;
    private CancellationTokenSource? _readCancellation;
    private Task? _readTask;
    private int _frequencyCorrection;

    public RtlSdrTuner(int deviceIndex = 0, ILogger<RtlSdrTuner>? logger = null)
        : base(logger)
    {
        _deviceIndex = deviceIndex;
        _sampleRate = RtlSdrConstants.DEFAULT_SAMPLE_RATE;
        _frequencyHz = 100000000; // 100 MHz default
    }

    /// <inheritdoc/>
    public override TunerType TunerType => TunerType.RtlSdr;

    /// <inheritdoc/>
    public override string Name => $"RTL-SDR #{_deviceIndex}";

    /// <inheritdoc/>
    public override string SerialNumber => _serialNumber;

    /// <inheritdoc/>
    public override long MinimumFrequencyHz => RtlSdrConstants.MIN_FREQUENCY_HZ;

    /// <inheritdoc/>
    public override long MaximumFrequencyHz => RtlSdrConstants.MAX_FREQUENCY_HZ;

    /// <inheritdoc/>
    public override int MinimumSampleRate => RtlSdrConstants.MIN_SAMPLE_RATE;

    /// <inheritdoc/>
    public override int MaximumSampleRate => RtlSdrConstants.MAX_SAMPLE_RATE;

    /// <inheritdoc/>
    public override double[] AvailableGains
    {
        get
        {
            // Convert from tenths of dB to dB
            return RtlSdrConstants.SupportedGains.Select(g => g / 10.0).ToArray();
        }
    }

    /// <inheritdoc/>
    public override async Task<bool> ConnectAsync()
    {
        try
        {
            _logger?.LogInformation("Attempting to connect to RTL-SDR device #{Index}", _deviceIndex);

            // Find RTL-SDR device
            var device = FindRtlSdrDevice(_deviceIndex);
            if (device == null)
            {
                _logger?.LogError("RTL-SDR device #{Index} not found", _deviceIndex);
                Status = TunerStatus.Error;
                return false;
            }

            _usbDevice = device;

            // Open the device
            if (!_usbDevice.Open())
            {
                _logger?.LogError("Failed to open RTL-SDR device #{Index}", _deviceIndex);
                Status = TunerStatus.Error;
                return false;
            }

            // Get serial number
            _serialNumber = _usbDevice.Info.SerialString ?? "Unknown";

            // Set configuration
            if (_usbDevice.Configs.Count > 0)
            {
                var config = _usbDevice.Configs[0];
                _usbDevice.SetConfiguration(config.Descriptor.ConfigID);
            }

            // Claim interface
            var wholeUsbDevice = _usbDevice as IUsbDevice;
            if (wholeUsbDevice != null)
            {
                wholeUsbDevice.ClaimInterface(0);
            }

            // Initialize the device
            await InitializeDeviceAsync();

            // Get bulk endpoint reader
            _bulkReader = _usbDevice.OpenEndpointReader(
                (ReadEndpointID)RtlSdrConstants.BULK_ENDPOINT,
                RtlSdrConstants.DEFAULT_BUFFER_LENGTH);

            Status = TunerStatus.Connected;
            _logger?.LogInformation("Successfully connected to RTL-SDR device {Serial}", _serialNumber);

            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error connecting to RTL-SDR device #{Index}", _deviceIndex);
            Status = TunerStatus.Error;
            return false;
        }
    }

    /// <inheritdoc/>
    public override async Task DisconnectAsync()
    {
        try
        {
            await StopAsync();

            if (_usbDevice != null)
            {
                // Release interface
                var wholeUsbDevice = _usbDevice as IUsbDevice;
                if (wholeUsbDevice != null && wholeUsbDevice.IsOpen)
                {
                    wholeUsbDevice.ReleaseInterface(0);
                }

                _usbDevice.Close();
                _usbDevice = null;
            }

            _bulkReader?.Dispose();
            _bulkReader = null;

            Status = TunerStatus.Disconnected;
            _logger?.LogInformation("Disconnected from RTL-SDR device");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error disconnecting from RTL-SDR device");
        }

        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override async Task StartAsync()
    {
        if (!IsConnected)
        {
            throw new TunerException("Tuner not connected");
        }

        if (IsRunning)
        {
            _logger?.LogWarning("Tuner is already running");
            return;
        }

        try
        {
            // Apply current settings
            await SetFrequencyAsync(_frequencyHz);
            await SetSampleRateAsync(_sampleRate);
            await SetGainAsync(_gain);

            // Reset endpoint
            ResetBuffer();

            // Start reading samples
            _readCancellation = new CancellationTokenSource();
            _readTask = Task.Run(() => ReadSamplesAsync(_readCancellation.Token));

            Status = TunerStatus.Running;
            _logger?.LogInformation("Started RTL-SDR tuner");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error starting RTL-SDR tuner");
            Status = TunerStatus.Error;
            throw new TunerException("Failed to start tuner", ex);
        }

        await Task.CompletedTask;
    }

    /// <inheritdoc/>
    public override async Task StopAsync()
    {
        if (!IsRunning)
        {
            return;
        }

        try
        {
            // Cancel reading
            _readCancellation?.Cancel();

            // Wait for read task to complete
            if (_readTask != null)
            {
                await _readTask;
                _readTask = null;
            }

            _readCancellation?.Dispose();
            _readCancellation = null;

            Status = TunerStatus.Connected;
            _logger?.LogInformation("Stopped RTL-SDR tuner");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error stopping RTL-SDR tuner");
        }
    }

    /// <inheritdoc/>
    public override async Task ResetAsync()
    {
        if (!IsConnected)
        {
            return;
        }

        try
        {
            await StopAsync();
            ResetBuffer();
            _logger?.LogInformation("Reset RTL-SDR tuner");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error resetting RTL-SDR tuner");
        }
    }

    /// <inheritdoc/>
    public override void SetFrequencyCorrection(int ppm)
    {
        _frequencyCorrection = ppm;
        _logger?.LogDebug("Set frequency correction to {PPM} PPM", ppm);
    }

    private async Task InitializeDeviceAsync()
    {
        // Reset the device
        WriteRegister(RtlSdrConstants.BLOCK_USB, 0, 0x00, 2);

        // Disable zero-copy mode
        WriteRegister(RtlSdrConstants.BLOCK_USB, 0, 0x00, 0);

        await Task.CompletedTask;
    }

    private async Task SetFrequencyAsync(long frequencyHz)
    {
        if (!IsConnected)
        {
            return;
        }

        // Apply frequency correction
        var correctedFreq = frequencyHz * (1.0 + _frequencyCorrection / 1000000.0);
        var freqValue = (uint)correctedFreq;

        // Set frequency (simplified - real implementation would tune the internal tuner chip)
        _logger?.LogDebug("Setting frequency to {Frequency} Hz", freqValue);

        await Task.CompletedTask;
    }

    private async Task SetSampleRateAsync(int sampleRate)
    {
        if (!IsConnected)
        {
            return;
        }

        _logger?.LogDebug("Setting sample rate to {SampleRate} Hz", sampleRate);

        // Calculate register values for sample rate
        // RTL2832 uses a ratio to set sample rate
        const uint rtlXtalFreq = 28800000;
        var ratio = (rtlXtalFreq * Math.Pow(2, 22)) / sampleRate;
        var ratioValue = (uint)ratio;

        WriteRegister(RtlSdrConstants.BLOCK_USB, 0, (short)(ratioValue & 0xFFFF), 0x009f);
        WriteRegister(RtlSdrConstants.BLOCK_USB, 0, (short)((ratioValue >> 16) & 0xFFFF), 0x00a1);

        await Task.CompletedTask;
    }

    private async Task SetGainAsync(double gainDb)
    {
        if (!IsConnected)
        {
            return;
        }

        // Convert gain to tenths of dB
        var gainTenthsDb = (int)(gainDb * 10);

        // Find closest supported gain
        var closestGain = RtlSdrConstants.SupportedGains
            .OrderBy(g => Math.Abs(g - gainTenthsDb))
            .First();

        _logger?.LogDebug("Setting gain to {Gain} dB", closestGain / 10.0);

        await Task.CompletedTask;
    }

    private void WriteRegister(byte block, byte reg, short value, short index)
    {
        if (_usbDevice == null)
        {
            return;
        }

        var setupPacket = new UsbSetupPacket(
            RtlSdrConstants.CTRL_OUT,
            0,
            value,
            (short)((reg << 8) | block),
            0);

        int bytesTransferred;
        _usbDevice.ControlTransfer(ref setupPacket, null, 0, out bytesTransferred);
    }

    private void ResetBuffer()
    {
        WriteRegister(RtlSdrConstants.BLOCK_USB, 0, 0x00, 2);
    }

    private async Task ReadSamplesAsync(CancellationToken cancellationToken)
    {
        if (_bulkReader == null)
        {
            return;
        }

        var buffer = new byte[RtlSdrConstants.DEFAULT_BUFFER_LENGTH];

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Read samples from USB bulk endpoint
                var errorCode = _bulkReader.Read(buffer, 5000, out int bytesRead);

                if (errorCode != ErrorCode.None || bytesRead == 0)
                {
                    if (errorCode == ErrorCode.IoTimedOut)
                    {
                        continue;
                    }

                    _logger?.LogWarning("USB read error: {Error}", errorCode);
                    break;
                }

                // Convert 8-bit unsigned samples to complex floats
                // RTL-SDR provides I/Q samples as interleaved unsigned 8-bit values
                var sampleCount = bytesRead / 2;
                var complexSamples = new Complex[sampleCount];

                for (int i = 0; i < sampleCount; i++)
                {
                    // Convert 8-bit unsigned to float (-1.0 to 1.0)
                    var iSample = (buffer[i * 2] - 127.5f) / 127.5f;
                    var qSample = (buffer[i * 2 + 1] - 127.5f) / 127.5f;
                    complexSamples[i] = new Complex(iSample, qSample);
                }

                // Create sample buffer and raise event
                var sampleBuffer = new ComplexSampleBuffer(
                    complexSamples,
                    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    _sampleRate);

                OnSamplesAvailable(sampleBuffer);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error reading samples from RTL-SDR");
            Status = TunerStatus.Error;
        }

        await Task.CompletedTask;
    }

    private static UsbDevice? FindRtlSdrDevice(int deviceIndex)
    {
        var devices = UsbDevice.AllDevices;
        int foundCount = 0;

        foreach (UsbRegistry regDevice in devices)
        {
            // Check if this is an RTL-SDR device
            var isRtlSdr = RtlSdrConstants.SupportedDevices
                .Any(d => d.VendorId == regDevice.Vid && d.ProductId == regDevice.Pid);

            if (isRtlSdr)
            {
                if (foundCount == deviceIndex)
                {
                    if (regDevice.Open(out UsbDevice? device))
                    {
                        return device;
                    }
                }
                foundCount++;
            }
        }

        return null;
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                DisconnectAsync().GetAwaiter().GetResult();
            }
        }

        base.Dispose(disposing);
    }
}
