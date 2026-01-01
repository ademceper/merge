namespace Merge.Application.Configuration;

/// <summary>
/// Order işlemleri için configuration ayarları
/// </summary>
public class OrderSettings
{
    public const string SectionName = "OrderSettings";

    /// <summary>
    /// Ücretsiz kargo için minimum sepet tutarı (TL)
    /// </summary>
    public decimal FreeShippingThreshold { get; set; } = 500;

    /// <summary>
    /// KDV oranı (0.18 = %18)
    /// </summary>
    public decimal TaxRate { get; set; } = 0.18m;

    /// <summary>
    /// Varsayılan kargo maliyeti (TL)
    /// </summary>
    public decimal DefaultShippingCost { get; set; } = 50;
}

