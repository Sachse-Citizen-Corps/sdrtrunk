using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SDRTrunk.Models;

/// <summary>
/// Represents an alias for a radio identifier (talkgroup, radio ID, etc.)
/// Converted from Java io.github.dsheirer.alias.Alias
/// </summary>
public class Alias : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private string _list = string.Empty;
    private string _group = string.Empty;
    private string _color = "#FFFFFF";
    private int _priority;
    private List<AliasId> _aliasIds = new();

    /// <summary>
    /// Alias name/label
    /// </summary>
    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    /// <summary>
    /// Alias list this alias belongs to
    /// </summary>
    public string List
    {
        get => _list;
        set => SetField(ref _list, value);
    }

    /// <summary>
    /// Group within the alias list
    /// </summary>
    public string Group
    {
        get => _group;
        set => SetField(ref _group, value);
    }

    /// <summary>
    /// Color for display (hex color code)
    /// </summary>
    public string Color
    {
        get => _color;
        set => SetField(ref _color, value);
    }

    /// <summary>
    /// Priority for audio playback (higher = more important)
    /// </summary>
    public int Priority
    {
        get => _priority;
        set => SetField(ref _priority, value);
    }

    /// <summary>
    /// List of identifiers this alias matches
    /// </summary>
    public List<AliasId> AliasIds
    {
        get => _aliasIds;
        set => SetField(ref _aliasIds, value);
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
/// Represents an identifier that an alias matches
/// </summary>
public class AliasId
{
    /// <summary>
    /// Type of identifier (Talkgroup, RadioId, etc.)
    /// </summary>
    public AliasIdType Type { get; set; }

    /// <summary>
    /// The identifier value
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Protocol this ID applies to
    /// </summary>
    public string? Protocol { get; set; }
}

/// <summary>
/// Types of alias identifiers
/// </summary>
public enum AliasIdType
{
    Talkgroup,
    RadioId,
    LtrNetUid,
    MptId,
    Fleetsync,
    Mdc1200,
    Site,
    Status,
    Custom
}
