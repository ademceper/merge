namespace Merge.Application.Configuration;

/// <summary>
/// Two Factor Authentication işlemleri için configuration ayarları
/// BOLUM 12.1: Magic Number Sorunu - Tüm magic number'lar configuration'a taşındı
/// </summary>
public class TwoFactorAuthSettings
{
    public const string SectionName = "TwoFactorAuth";

    /// <summary>
    /// Maksimum başarısız 2FA doğrulama denemesi (hesap kilitleme için)
    /// </summary>
    public int MaxFailedAttempts { get; set; } = 5;

    /// <summary>
    /// Hesap kilitleme süresi (dakika) - Başarısız deneme sonrası
    /// </summary>
    public int LockoutMinutes { get; set; } = 15;

    /// <summary>
    /// 2FA doğrulama kodu uzunluğu (karakter sayısı)
    /// </summary>
    public int VerificationCodeLength { get; set; } = 6;

    /// <summary>
    /// 2FA doğrulama kodu geçerlilik süresi (dakika)
    /// </summary>
    public int VerificationCodeExpirationMinutes { get; set; } = 5;

    /// <summary>
    /// Backup kod sayısı
    /// </summary>
    public int BackupCodeCount { get; set; } = 10;

    /// <summary>
    /// TOTP time step (saniye) - Genelde 30 saniye
    /// </summary>
    public int TotpTimeStepSeconds { get; set; } = 30;

    /// <summary>
    /// Tarih formatı string'i (örn: yyyy-MM-dd HH:mm:ss)
    /// </summary>
    public string DateTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss";
}

