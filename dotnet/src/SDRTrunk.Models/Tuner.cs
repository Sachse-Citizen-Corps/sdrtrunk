using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SDRTrunk.Models;

/// <summary>
/// Represents an SDR tuner device (RTL-SDR, HackRF, Airspy, etc.)
/// </summary>
public class Tuner : INotifyPropertyChanged
{
    private int _tunerId;
    private string _name = string.Empty;
    private TunerType _tunerType;
    private string _serialNumber = string.Empty;
    private long _frequencyHz;
    private int _sampleRate;
    private double _gainDb;
    private bool _isConnected;
    private TunerStatus _status = TunerStatus.Disconnected;

    /// <summary>
    /// Unique identifier for this tuner
    /// </summary>
    public int TunerId
    {
        get => _tunerId;
        set => SetField(ref _tunerId, value);
    }

    /// <summary>
    /// Tuner name/label
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    /// Type of SDR tuner
    /// </summary>
    public TunerType TunerType
    {
        get => _tunerType;
        set => SetField(ref _tunerType, value);
    }

    /// <summary>
    /// Serial number or unique device identifier
    /// </summary>
    public string SerialNumber
    {
        get => _serialNumber;
        set => SetField(ref _serialNumber, value);
    }

    /// <summary>
    /// Current center frequency in Hz
    /// </summary>
    public long FrequencyHz
    {
        get => _frequencyHz;
        set => SetField(ref _frequencyHz, value);
    }

    /// <summary>
    /// Sample rate in samples per second
    /// </summary>
    public int SampleRate
    {
        get => _sampleRate;
        set => SetField(ref _sampleRate, value);
    }

    /// <summary>
    /// Gain in decibels
    /// </summary>
    public double GainDb
    {
        get => _gainDb;
        set => SetField(ref _gainDb, value);
    }

    /// <summary>
    /// Whether tuner is connected
    /// </summary>
    public bool IsConnected
    {
        get => _isConnected;
        set => SetField(ref _isConnected, value);
    }

    /// <summary>
    /// Current tuner status
    /// </summary>
    public TunerStatus Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

/// <summary>
/// SDR tuner types supported
/// </summary>
public enum TunerType
{
    Unknown,
    RtlSdr,
    HackRF,
    Airspy,
    AirspyHfPlus,
    SdrPlay,
    FunCubeDongle,
    Recording
}

/// <summary>
/// Tuner status
/// </summary>
public enum TunerStatus
{
    Disconnected,
    Connected,
    Running,
    Error
}
