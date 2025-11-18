using SDRTrunk.DSP.Buffer;
using SDRTrunk.Models;

namespace SDRTrunk.Tuners.Interfaces;

/// <summary>
/// Interface for SDR tuner hardware devices.
/// Converted from Java io.github.dsheirer.source.tuner.Tuner
/// </summary>
public interface ITuner : IDisposable
{
    /// <summary>
    /// Gets the tuner type (RTL-SDR, HackRF, etc.)
    /// </summary>
    TunerType TunerType { get; }

    /// <summary>
    /// Gets the tuner name/identifier
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the device serial number or unique identifier
    /// </summary>
    string SerialNumber { get; }

    /// <summary>
    /// Gets whether the tuner is currently connected
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets whether the tuner is currently running
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets or sets the center frequency in Hz
    /// </summary>
    long FrequencyHz { get; set; }

    /// <summary>
    /// Gets or sets the sample rate in Hz
    /// </summary>
    int SampleRate { get; set; }

    /// <summary>
    /// Gets the minimum supported frequency in Hz
    /// </summary>
    long MinimumFrequencyHz { get; }

    /// <summary>
    /// Gets the maximum supported frequency in Hz
    /// </summary>
    long MaximumFrequencyHz { get; }

    /// <summary>
    /// Gets the minimum supported sample rate in Hz
    /// </summary>
    int MinimumSampleRate { get; }

    /// <summary>
    /// Gets the maximum supported sample rate in Hz
    /// </summary>
    int MaximumSampleRate { get; }

    /// <summary>
    /// Gets or sets the tuner gain in dB
    /// </summary>
    double Gain { get; set; }

    /// <summary>
    /// Gets available gain values in dB
    /// </summary>
    double[] AvailableGains { get; }

    /// <summary>
    /// Gets or sets whether automatic gain control is enabled
    /// </summary>
    bool AutomaticGainControl { get; set; }

    /// <summary>
    /// Gets the current status of the tuner
    /// </summary>
    TunerStatus Status { get; }

    /// <summary>
    /// Event fired when new samples are available
    /// </summary>
    event EventHandler<ComplexSampleBuffer>? SamplesAvailable;

    /// <summary>
    /// Event fired when tuner status changes
    /// </summary>
    event EventHandler<TunerStatus>? StatusChanged;

    /// <summary>
    /// Connect to the tuner hardware
    /// </summary>
    /// <returns>True if connection successful</returns>
    Task<bool> ConnectAsync();

    /// <summary>
    /// Disconnect from the tuner hardware
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Start streaming samples from the tuner
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// Stop streaming samples from the tuner
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Reset the tuner to default state
    /// </summary>
    Task ResetAsync();

    /// <summary>
    /// Apply frequency correction in PPM (parts per million)
    /// </summary>
    /// <param name="ppm">Frequency correction in PPM</param>
    void SetFrequencyCorrection(int ppm);

    /// <summary>
    /// Get device information as a formatted string
    /// </summary>
    string GetDeviceInfo();
}
