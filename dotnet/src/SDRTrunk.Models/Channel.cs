using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SDRTrunk.Models;

/// <summary>
/// Represents a radio channel configuration.
/// Converted from Java io.github.dsheirer.controller.channel.Channel
/// </summary>
public class Channel : INotifyPropertyChanged
{
    private int _channelId;
    private string _name = string.Empty;
    private string _system = string.Empty;
    private string _site = string.Empty;
    private string _frequency = string.Empty;
    private string _bandwidth = string.Empty;
    private string _decoderType = "NBFM";
    private string _aliasListName = string.Empty;
    private bool _processing;
    private bool _autoStart;
    private int _autoStartOrder;
    private bool _selected;
    private bool _isActive;
    private ChannelType _channelType = ChannelType.Standard;

    /// <summary>
    /// Unique identifier for this channel (transient, not persisted)
    /// </summary>
    public int ChannelId
    {
        get => _channelId;
        set => SetField(ref _channelId, value);
    }

    /// <summary>
    /// Channel name
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    /// Owning system for the channel
    /// </summary>
    public string System
    {
        get => _system;
        set => SetField(ref _system, value);
    }

    /// <summary>
    /// Owning site for the channel
    /// </summary>
    public string Site
    {
        get => _site;
        set => SetField(ref _site, value);
    }

    /// <summary>
    /// Channel frequency
    /// </summary>
    public string Frequency
    {
        get => _frequency;
        set => SetField(ref _frequency, value);
    }

    /// <summary>
    /// Channel bandwidth
    /// </summary>
    public string Bandwidth
    {
        get => _bandwidth;
        set => SetField(ref _bandwidth, value);
    }

    /// <summary>
    /// Decoder type (NBFM, AM, P25, DMR, etc.)
    /// </summary>
    public string DecoderType
    {
        get => _decoderType;
        set => SetField(ref _decoderType, value);
    }

    /// <summary>
    /// Alias list name used for identifier lookups
    /// </summary>
    public string AliasListName
    {
        get => _aliasListName;
        set => SetField(ref _aliasListName, value);
    }

    /// <summary>
    /// Whether channel is currently processing
    /// </summary>
    public bool Processing
    {
        get => _processing;
        set => SetField(ref _processing, value);
    }

    /// <summary>
    /// Whether channel should auto-start on application startup
    /// </summary>
    public bool AutoStart
    {
        get => _autoStart;
        set => SetField(ref _autoStart, value);
    }

    /// <summary>
    /// Order for starting auto-start channels
    /// </summary>
    public int AutoStartOrder
    {
        get => _autoStartOrder;
        set => SetField(ref _autoStartOrder, value);
    }

    /// <summary>
    /// Whether channel is selected for prioritized output
    /// </summary>
    public bool Selected
    {
        get => _selected;
        set => SetField(ref _selected, value);
    }

    /// <summary>
    /// Whether channel is currently active
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (SetField(ref _isActive, value))
            {
                OnPropertyChanged(nameof(IsActiveColor));
            }
        }
    }

    /// <summary>
    /// Color indicator for channel active state (for UI binding)
    /// </summary>
    public string IsActiveColor => IsActive ? "#00FF00" : "#808080";

    /// <summary>
    /// Channel type (Standard or Traffic)
    /// </summary>
    public ChannelType ChannelType
    {
        get => _channelType;
        set => SetField(ref _channelType, value);
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
/// Channel type enumeration
/// </summary>
public enum ChannelType
{
    /// <summary>
    /// Standard persisted channel
    /// </summary>
    Standard,

    /// <summary>
    /// Temporary traffic channel
    /// </summary>
    Traffic
}
