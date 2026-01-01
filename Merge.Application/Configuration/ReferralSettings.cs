namespace Merge.Application.Configuration;

/// <summary>
/// Referral (Referans) programı için configuration ayarları
/// </summary>
public class ReferralSettings
{
    public const string SectionName = "ReferralSettings";

    /// <summary>
    /// Referans veren kullanıcıya verilen puan
    /// </summary>
    public int ReferrerPointsReward { get; set; } = 100;

    /// <summary>
    /// Referans edilen kullanıcıya verilen indirim yüzdesi
    /// </summary>
    public int RefereeDiscountPercentage { get; set; } = 10;
}

