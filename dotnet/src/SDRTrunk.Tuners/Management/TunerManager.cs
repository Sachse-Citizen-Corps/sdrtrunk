using System.Collections.ObjectModel;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using Microsoft.Extensions.Logging;
using SDRTrunk.Core.Services;
using SDRTrunk.Models;
using SDRTrunk.Tuners.Interfaces;
using SDRTrunk.Tuners.RtlSdr;

namespace SDRTrunk.Tuners.Management;

/// <summary>
/// Manages discovery and lifecycle of SDR tuner devices
/// </summary>
public class TunerManager : IDisposable
{
    private readonly ILogger<TunerManager>? _logger;
    private readonly IEventBus _eventBus;
    private readonly ObservableCollection<ITuner> _tuners = new();
    private readonly object _lock = new();
    private bool _disposed;

    public TunerManager(IEventBus eventBus, ILogger<TunerManager>? logger = null)
    {
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _logger = logger;

        _logger?.LogInformation("TunerManager initialized");
    }

    /// <summary>
    /// Gets the collection of discovered tuners
    /// </summary>
    public ObservableCollection<ITuner> Tuners => _tuners;

    /// <summary>
    /// Gets the count of connected tuners
    /// </summary>
    public int TunerCount => _tuners.Count;

    /// <summary>
    /// Discover all available SDR tuners
    /// </summary>
    /// <returns>Number of tuners discovered</returns>
    public async Task<int> DiscoverTunersAsync()
    {
        lock (_lock)
        {
            _logger?.LogInformation("Starting tuner discovery...");

            // Clear existing tuners
            foreach (var tuner in _tuners)
            {
                tuner.Dispose();
            }
            _tuners.Clear();

            // Discover RTL-SDR devices
            var rtlSdrCount = DiscoverRtlSdrTuners();

            // Future: Add discovery for other tuner types (HackRF, Airspy, etc.)

            _logger?.LogInformation("Tuner discovery complete. Found {Count} tuner(s)", _tuners.Count);

            return _tuners.Count;
        }
    }

    /// <summary>
    /// Get a tuner by index
    /// </summary>
    public ITuner? GetTuner(int index)
    {
        lock (_lock)
        {
            if (index >= 0 && index < _tuners.Count)
            {
                return _tuners[index];
            }
            return null;
        }
    }

    /// <summary>
    /// Get a tuner by serial number
    /// </summary>
    public ITuner? GetTunerBySerial(string serialNumber)
    {
        lock (_lock)
        {
            return _tuners.FirstOrDefault(t => t.SerialNumber == serialNumber);
        }
    }

    /// <summary>
    /// Get all tuners of a specific type
    /// </summary>
    public IEnumerable<ITuner> GetTunersByType(TunerType type)
    {
        lock (_lock)
        {
            return _tuners.Where(t => t.TunerType == type).ToList();
        }
    }

    /// <summary>
    /// Connect to all discovered tuners
    /// </summary>
    public async Task<int> ConnectAllTunersAsync()
    {
        var connectedCount = 0;

        foreach (var tuner in _tuners)
        {
            try
            {
                if (await tuner.ConnectAsync())
                {
                    connectedCount++;
                    _logger?.LogInformation("Connected to tuner: {Name}", tuner.Name);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to connect to tuner: {Name}", tuner.Name);
            }
        }

        return connectedCount;
    }

    /// <summary>
    /// Disconnect from all tuners
    /// </summary>
    public async Task DisconnectAllTunersAsync()
    {
        foreach (var tuner in _tuners)
        {
            try
            {
                await tuner.DisconnectAsync();
                _logger?.LogInformation("Disconnected from tuner: {Name}", tuner.Name);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error disconnecting from tuner: {Name}", tuner.Name);
            }
        }
    }

    private int DiscoverRtlSdrTuners()
    {
        var count = 0;

        try
        {
            var devices = UsbDevice.AllDevices;

            foreach (UsbRegistry regDevice in devices)
            {
                // Check if this is an RTL-SDR device
                var deviceInfo = RtlSdrConstants.SupportedDevices
                    .FirstOrDefault(d => d.VendorId == regDevice.Vid && d.ProductId == regDevice.Pid);

                if (deviceInfo.Name != null)
                {
                    _logger?.LogDebug("Found RTL-SDR device: {Name} (VID: 0x{VID:X4}, PID: 0x{PID:X4})",
                        deviceInfo.Name, regDevice.Vid, regDevice.Pid);

                    var tuner = new RtlSdrTuner(count, _logger as ILogger<RtlSdrTuner>);
                    _tuners.Add(tuner);
                    count++;
                }
            }

            _logger?.LogInformation("Discovered {Count} RTL-SDR device(s)", count);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error discovering RTL-SDR devices");
        }

        return count;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            DisconnectAllTunersAsync().GetAwaiter().GetResult();

            foreach (var tuner in _tuners)
            {
                tuner.Dispose();
            }

            _tuners.Clear();
            _disposed = true;
        }
    }
}

/// <summary>
/// Event arguments for tuner discovery
/// </summary>
public class TunerDiscoveredEventArgs : EventArgs
{
    public ITuner Tuner { get; }

    public TunerDiscoveredEventArgs(ITuner tuner)
    {
        Tuner = tuner ?? throw new ArgumentNullException(nameof(tuner));
    }
}
