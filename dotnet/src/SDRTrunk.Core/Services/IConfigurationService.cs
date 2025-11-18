namespace SDRTrunk.Core.Services;

/// <summary>
/// Service for managing application configuration and settings.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Get a configuration value by key.
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">Configuration key</param>
    /// <param name="defaultValue">Default value if key not found</param>
    /// <returns>The configuration value or default</returns>
    T GetValue<T>(string key, T defaultValue);

    /// <summary>
    /// Set a configuration value.
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <param name="key">Configuration key</param>
    /// <param name="value">Value to set</param>
    void SetValue<T>(string key, T value);

    /// <summary>
    /// Save configuration to disk.
    /// </summary>
    void Save();

    /// <summary>
    /// Load configuration from disk.
    /// </summary>
    void Load();
}
