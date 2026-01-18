using Merge.Domain.Modules.Identity;
namespace Merge.Application.Configuration;


public class UserSettings
{
    public const string SectionName = "UserSettings";

    /// <summary>
    /// User Activity ayarları
    /// </summary>
    public ActivitySettings Activity { get; set; } = new();

    /// <summary>
    /// User Preference ayarları
    /// </summary>
    public PreferenceSettings Preference { get; set; } = new();
}

/// <summary>
/// User Activity ayarları
/// </summary>
public class ActivitySettings
{
    /// <summary>
    /// Session timeout (dakika) - default: 30
    /// </summary>
    public int SessionTimeoutMinutes { get; set; } = 30;

    /// <summary>
    /// Varsayılan gün sayısı - default: 30
    /// </summary>
    public int DefaultDays { get; set; } = 30;

    /// <summary>
    /// Maksimum gün sayısı - default: 365
    /// </summary>
    public int MaxDays { get; set; } = 365;

    /// <summary>
    /// Session için maksimum gün sayısı - default: 90
    /// </summary>
    public int MaxSessionDays { get; set; } = 90;

    /// <summary>
    /// Varsayılan session gün sayısı - default: 7
    /// </summary>
    public int DefaultSessionDays { get; set; } = 7;

    /// <summary>
    /// Varsayılan top N değeri - default: 10
    /// </summary>
    public int DefaultTopN { get; set; } = 10;

    /// <summary>
    /// Maksimum top N değeri - default: 100
    /// </summary>
    public int MaxTopN { get; set; } = 100;

    /// <summary>
    /// Eski aktiviteleri silme - minimum tutulacak gün sayısı - default: 90
    /// </summary>
    public int MinDaysToKeep { get; set; } = 90;

    /// <summary>
    /// Eski aktiviteleri silme - maksimum tutulacak gün sayısı - default: 365
    /// </summary>
    public int MaxDaysToKeep { get; set; } = 365;

    /// <summary>
    /// Session başına ortalama aktivite sayısı tahmini (List capacity için) - default: 10
    /// </summary>
    public int AverageActivitiesPerSession { get; set; } = 10;
}

/// <summary>
/// User Preference ayarları
/// </summary>
public class PreferenceSettings
{
    /// <summary>
    /// Varsayılan ItemsPerPage - default: 20
    /// </summary>
    public int DefaultItemsPerPage { get; set; } = 20;

    /// <summary>
    /// Minimum ItemsPerPage - default: 1
    /// </summary>
    public int MinItemsPerPage { get; set; } = 1;

    /// <summary>
    /// Maksimum ItemsPerPage - default: 100
    /// </summary>
    public int MaxItemsPerPage { get; set; } = 100;
}
