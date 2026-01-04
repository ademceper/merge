namespace Merge.Application.Configuration;

/// <summary>
/// Support/Ticket islemleri icin configuration ayarlari
/// </summary>
public class SupportSettings
{
    public const string SectionName = "SupportSettings";

    /// <summary>
    /// Varsayilan istatistik periyodu (gun)
    /// </summary>
    public int DefaultStatsPeriodDays { get; set; } = 30;

    /// <summary>
    /// Haftalik rapor periyodu (gun)
    /// </summary>
    public int WeeklyReportDays { get; set; } = 7;

    /// <summary>
    /// SLA uyari esigi (saat)
    /// </summary>
    public int SlaWarningThresholdHours { get; set; } = 24;
}
