namespace Merge.Application.Configuration;

/// <summary>
/// Cart işlemleri için configuration ayarları
/// BOLUM 2.3: Hardcoded Values YASAK (Configuration Kullan)
/// </summary>
public class CartSettings
{
    public const string SectionName = "CartSettings";

    /// <summary>
    /// Terk edilmiş sepet kurtarma e-postası için varsayılan kupon indirim yüzdesi
    /// </summary>
    public decimal DefaultAbandonedCartCouponDiscount { get; set; } = 10;

    /// <summary>
    /// Son görüntülenen ürünler için maksimum kayıt sayısı
    /// </summary>
    public int MaxRecentlyViewedItems { get; set; } = 100;

    /// <summary>
    /// Terk edilmiş sepet için varsayılan kupon geçerlilik süresi (gün)
    /// </summary>
    public int AbandonedCartCouponValidityDays { get; set; } = 7;
}

