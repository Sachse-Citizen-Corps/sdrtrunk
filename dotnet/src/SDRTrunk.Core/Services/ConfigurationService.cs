using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SDRTrunk.Core.Services;

/// <summary>
/// Configuration service implementation that stores settings in JSON format.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly ConcurrentDictionary<string, object> _settings = new();
    private readonly string _configFilePath;
    private readonly ILogger<ConfigurationService>? _logger;
    private readonly object _saveLock = new();

    public ConfigurationService(ILogger<ConfigurationService>? logger = null)
    {
        _logger = logger;

        // Store configuration in user's home directory
        var configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SDRTrunk");

        Directory.CreateDirectory(configDir);
        _configFilePath = Path.Combine(configDir, "settings.json");

        Load();
    }

    /// <inheritdoc/>
    public T GetValue<T>(string key, T defaultValue)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

        if (_settings.TryGetValue(key, out var value))
        {
            try
            {
                // Handle JsonElement conversion from deserialization
                if (value is JsonElement jsonElement)
                {
                    return jsonElement.Deserialize<T>() ?? defaultValue;
                }

                // Direct type conversion
                if (value is T typedValue)
                {
                    return typedValue;
                }

                // Try to convert
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error converting configuration value for key {Key}, returning default", key);
                return defaultValue;
            }
        }

        return defaultValue;
    }

    /// <inheritdoc/>
    public void SetValue<T>(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

        if (value == null)
        {
            _settings.TryRemove(key, out _);
            _logger?.LogDebug("Removed configuration key {Key}", key);
        }
        else
        {
            _settings[key] = value;
            _logger?.LogDebug("Set configuration key {Key} to value {Value}", key, value);
        }
    }

    /// <inheritdoc/>
    public void Save()
    {
        lock (_saveLock)
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_configFilePath, json);
                _logger?.LogInformation("Configuration saved to {FilePath}", _configFilePath);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save configuration to {FilePath}", _configFilePath);
                throw;
            }
        }
    }

    /// <inheritdoc/>
    public void Load()
    {
        lock (_saveLock)
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    _logger?.LogInformation("Configuration file not found at {FilePath}, starting with empty configuration", _configFilePath);
                    return;
                }

                var json = File.ReadAllText(_configFilePath);
                var settings = JsonSerializer.Deserialize<ConcurrentDictionary<string, object>>(json);

                if (settings != null)
                {
                    _settings.Clear();
                    foreach (var kvp in settings)
                    {
                        _settings[kvp.Key] = kvp.Value;
                    }

                    _logger?.LogInformation("Configuration loaded from {FilePath} with {Count} settings",
                        _configFilePath, _settings.Count);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to load configuration from {FilePath}", _configFilePath);
            }
        }
    }
}
