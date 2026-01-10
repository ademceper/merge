namespace Merge.Application.Configuration;

/// <summary>
/// Marketing domain için configuration ayarları
/// Magic number'ları buraya taşıyoruz
/// </summary>
public class MarketingSettings
{
    public const string SectionName = "MarketingSettings";

    /// <summary>
    /// Pagination için maksimum sayfa boyutu
    /// </summary>
    public int MaxPageSize { get; set; } = 100;

    /// <summary>
    /// Varsayılan sayfa boyutu
    /// </summary>
    public int DefaultPageSize { get; set; } = 20;

    /// <summary>
    /// Loyalty transactions için maksimum gün sayısı
    /// </summary>
    public int MaxTransactionDays { get; set; } = 365;

    /// <summary>
    /// Loyalty transactions için varsayılan gün sayısı
    /// </summary>
    public int DefaultTransactionDays { get; set; } = 30;

    /// <summary>
    /// Loyalty points için varsayılan son kullanma yılı
    /// </summary>
    public int PointsExpiryYears { get; set; } = 10;

    /// <summary>
    /// Gift card için varsayılan son kullanma yılı
    /// </summary>
    public int GiftCardExpiryYears { get; set; } = 1;

    /// <summary>
    /// Gift card kod üretimi için minimum random sayı
    /// </summary>
    public int GiftCardCodeMinRandom { get; set; } = 1000;

    /// <summary>
    /// Gift card kod üretimi için maksimum random sayı
    /// </summary>
    public int GiftCardCodeMaxRandom { get; set; } = 9999;

    /// <summary>
    /// Referral kod üretimi için minimum random sayı
    /// </summary>
    public int ReferralCodeMinRandom { get; set; } = 1000;

    /// <summary>
    /// Referral kod üretimi için maksimum random sayı
    /// </summary>
    public int ReferralCodeMaxRandom { get; set; } = 9999;
}
