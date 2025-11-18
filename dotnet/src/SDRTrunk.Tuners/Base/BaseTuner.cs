using Microsoft.Extensions.Logging;
using SDRTrunk.DSP.Buffer;
using SDRTrunk.Models;
using SDRTrunk.Tuners.Interfaces;

namespace SDRTrunk.Tuners.Base;

/// <summary>
/// Base abstract class for all tuner implementations providing common functionality
/// </summary>
public abstract class BaseTuner : ITuner
{
    protected readonly ILogger? _logger;
    protected long _frequencyHz;
    protected int _sampleRate;
    protected double _gain;
    protected bool _automaticGainControl;
    protected TunerStatus _status = TunerStatus.Disconnected;
    protected bool _disposed;

    protected BaseTuner(ILogger? logger = null)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public abstract TunerType TunerType { get; }

    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public abstract string SerialNumber { get; }

    /// <inheritdoc/>
    public bool IsConnected => _status != TunerStatus.Disconnected;

    /// <inheritdoc/>
    public bool IsRunning => _status == TunerStatus.Running;

    /// <inheritdoc/>
    public virtual long FrequencyHz
    {
        get => _frequencyHz;
        set
        {
            if (value < MinimumFrequencyHz || value > MaximumFrequencyHz)
            {
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"Frequency must be between {MinimumFrequencyHz} and {MaximumFrequencyHz} Hz");
            }
            _frequencyHz = value;
        }
    }

    /// <inheritdoc/>
    public virtual int SampleRate
    {
        get => _sampleRate;
        set
        {
            if (value < MinimumSampleRate || value > MaximumSampleRate)
            {
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"Sample rate must be between {MinimumSampleRate} and {MaximumSampleRate} Hz");
            }
            _sampleRate = value;
        }
    }

    /// <inheritdoc/>
    public abstract long MinimumFrequencyHz { get; }

    /// <inheritdoc/>
    public abstract long MaximumFrequencyHz { get; }

    /// <inheritdoc/>
    public abstract int MinimumSampleRate { get; }

    /// <inheritdoc/>
    public abstract int MaximumSampleRate { get; }

    /// <inheritdoc/>
    public virtual double Gain
    {
        get => _gain;
        set => _gain = value;
    }

    /// <inheritdoc/>
    public abstract double[] AvailableGains { get; }

    /// <inheritdoc/>
    public virtual bool AutomaticGainControl
    {
        get => _automaticGainControl;
        set => _automaticGainControl = value;
    }

    /// <inheritdoc/>
    public TunerStatus Status
    {
        get => _status;
        protected set
        {
            if (_status != value)
            {
                _status = value;
                OnStatusChanged(value);
            }
        }
    }

    /// <inheritdoc/>
    public event EventHandler<ComplexSampleBuffer>? SamplesAvailable;

    /// <inheritdoc/>
    public event EventHandler<TunerStatus>? StatusChanged;

    /// <inheritdoc/>
    public abstract Task<bool> ConnectAsync();

    /// <inheritdoc/>
    public abstract Task DisconnectAsync();

    /// <inheritdoc/>
    public abstract Task StartAsync();

    /// <inheritdoc/>
    public abstract Task StopAsync();

    /// <inheritdoc/>
    public abstract Task ResetAsync();

    /// <inheritdoc/>
    public abstract void SetFrequencyCorrection(int ppm);

    /// <inheritdoc/>
    public virtual string GetDeviceInfo()
    {
        return $"{TunerType} - {Name} (S/N: {SerialNumber})";
    }

    /// <summary>
    /// Raises the SamplesAvailable event
    /// </summary>
    protected virtual void OnSamplesAvailable(ComplexSampleBuffer buffer)
    {
        SamplesAvailable?.Invoke(this, buffer);
    }

    /// <summary>
    /// Raises the StatusChanged event
    /// </summary>
    protected virtual void OnStatusChanged(TunerStatus status)
    {
        _logger?.LogDebug("Tuner {Name} status changed to {Status}", Name, status);
        StatusChanged?.Invoke(this, status);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                DisconnectAsync().GetAwaiter().GetResult();
            }

            _disposed = true;
        }
    }

    ~BaseTuner()
    {
        Dispose(false);
    }
}
