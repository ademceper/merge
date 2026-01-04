namespace Merge.Application.Configuration;

/// <summary>
/// Guvenlik islemleri icin configuration ayarlari
/// </summary>
public class SecuritySettings
{
    public const string SectionName = "SecuritySettings";

    /// <summary>
    /// Varsayilan istatistik periyodu (gun)
    /// </summary>
    public int DefaultStatsPeriodDays { get; set; } = 30;

    /// <summary>
    /// Maksimum basarisiz giris denemesi
    /// </summary>
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>
    /// Hesap kilitleme suresi (dakika)
    /// </summary>
    public int AccountLockoutMinutes { get; set; } = 15;
}
