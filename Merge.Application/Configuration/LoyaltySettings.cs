namespace Merge.Application.Configuration;

/// <summary>
/// Loyalty (Sadakat) programı için configuration ayarları
/// </summary>
public class LoyaltySettings
{
    public const string SectionName = "LoyaltySettings";

    /// <summary>
    /// Kayıt bonusu puan miktarı
    /// </summary>
    public int SignupBonusPoints { get; set; } = 100;

    /// <summary>
    /// Her 1 TL için kazanılan puan
    /// </summary>
    public int PointsPerTL { get; set; } = 10;
}

