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

    /// <summary>
    /// Para birimi başına puan oranı (örn: 1.0 = $1 = 1 point)
    /// </summary>
    public decimal? PointsPerCurrencyUnit { get; set; } = 1.0m;

    /// <summary>
    /// Puan başına para birimi oranı (örn: 0.01 = 1 point = $0.01)
    /// </summary>
    public decimal? CurrencyPerPoint { get; set; } = 0.01m;

    /// <summary>
    /// Maksimum işlem günü (loyalty transactions için)
    /// </summary>
    public int MaxTransactionDays { get; set; } = 365;
}

