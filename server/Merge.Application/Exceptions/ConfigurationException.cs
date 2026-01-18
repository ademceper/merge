namespace Merge.Application.Exceptions;

/// <summary>
/// Configuration hatası exception'ı.
/// HTTP Status: 500 Internal Server Error
/// </summary>
public class ConfigurationException : MergeException
{
    public ConfigurationException(string message)
        : base("CONFIGURATION_ERROR", message, 500)
    {
    }

    public ConfigurationException(string message, Exception innerException)
        : base("CONFIGURATION_ERROR", message, 500, innerException)
    {
    }

    public ConfigurationException(string settingName, string message)
        : base("CONFIGURATION_ERROR", $"{settingName}: {message}", 500)
    {
        WithMetadata("settingName", settingName);
    }
}
