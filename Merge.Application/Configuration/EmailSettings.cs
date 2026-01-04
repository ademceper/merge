namespace Merge.Application.Configuration;

/// <summary>
/// Email islemleri icin configuration ayarlari
/// </summary>
public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    /// <summary>
    /// Email dogrulama token gecerlilik suresi (saat)
    /// </summary>
    public int VerificationTokenExpirationHours { get; set; } = 24;

    /// <summary>
    /// Sifre sifirlama token gecerlilik suresi (saat)
    /// </summary>
    public int PasswordResetTokenExpirationHours { get; set; } = 2;

    /// <summary>
    /// Davet token gecerlilik suresi (gun)
    /// </summary>
    public int InvitationTokenExpirationDays { get; set; } = 7;
}
