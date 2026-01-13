namespace Merge.Application.Configuration;

/// <summary>
/// Shipping işlemleri için configuration ayarları
/// ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma (ZORUNLU)
/// </summary>
public class ShippingSettings
{
    public const string SectionName = "ShippingSettings";

    /// <summary>
    /// Kargo sağlayıcıları ve maliyetleri
    /// </summary>
    public Dictionary<string, ShippingProviderConfig> Providers { get; set; } = new()
    {
        { "YURTICI", new ShippingProviderConfig { Name = "Yurtiçi Kargo", BaseCost = 50m, EstimatedDays = 3 } },
        { "ARAS", new ShippingProviderConfig { Name = "Aras Kargo", BaseCost = 45m, EstimatedDays = 2 } },
        { "MNG", new ShippingProviderConfig { Name = "MNG Kargo", BaseCost = 40m, EstimatedDays = 2 } },
        { "SURAT", new ShippingProviderConfig { Name = "Sürat Kargo", BaseCost = 55m, EstimatedDays = 3 } }
    };

    /// <summary>
    /// Varsayılan kargo maliyeti (TL)
    /// </summary>
    public decimal DefaultShippingCost { get; set; } = 50m;

    /// <summary>
    /// Ücretsiz kargo için minimum sepet tutarı (TL)
    /// Bu değer OrderSettings.FreeShippingThreshold ile aynı olmalı
    /// </summary>
    public decimal FreeShippingThreshold { get; set; } = 500m;

    /// <summary>
    /// Varsayılan teslimat süresi ayarları (gün)
    /// </summary>
    public DefaultDeliveryTimeSettings DefaultDeliveryTime { get; set; } = new();

    /// <summary>
    /// Query limit ayarları (unbounded query koruması için)
    /// </summary>
    public QueryLimitSettings QueryLimits { get; set; } = new();
}

/// <summary>
/// Query limit ayarları - Unbounded query koruması için
/// </summary>
public class QueryLimitSettings
{
    /// <summary>
    /// Maksimum depo sayısı (GetAllWarehouses, GetActiveWarehouses)
    /// </summary>
    public int MaxWarehouses { get; set; } = 1000;

    /// <summary>
    /// Maksimum pick-pack sayısı (GetPickPacksByOrderId)
    /// </summary>
    public int MaxPickPacksPerOrder { get; set; } = 50;

    /// <summary>
    /// Maksimum teslimat tahmini sayısı (GetAllDeliveryTimeEstimations)
    /// </summary>
    public int MaxDeliveryTimeEstimations { get; set; } = 500;

    /// <summary>
    /// Maksimum kullanıcı adresi sayısı (GetUserShippingAddresses)
    /// </summary>
    public int MaxShippingAddressesPerUser { get; set; } = 50;

    /// <summary>
    /// Maksimum stok hareketi sayısı (GetStockMovementsByInventoryId)
    /// </summary>
    public int MaxStockMovementsPerInventory { get; set; } = 100;

    /// <summary>
    /// Maksimum sayfa boyutu (pagination için)
    /// </summary>
    public int MaxPageSize { get; set; } = 100;
}

/// <summary>
/// Varsayılan teslimat süresi ayarları
/// </summary>
public class DefaultDeliveryTimeSettings
{
    /// <summary>
    /// Varsayılan minimum teslimat süresi (gün)
    /// </summary>
    public int MinDays { get; set; } = 3;

    /// <summary>
    /// Varsayılan maksimum teslimat süresi (gün)
    /// </summary>
    public int MaxDays { get; set; } = 7;

    /// <summary>
    /// Varsayılan ortalama teslimat süresi (gün)
    /// </summary>
    public int AverageDays { get; set; } = 5;
}

/// <summary>
/// Kargo sağlayıcı konfigürasyonu
/// </summary>
public class ShippingProviderConfig
{
    /// <summary>
    /// Kargo firması adı
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Temel kargo maliyeti (TL)
    /// </summary>
    public decimal BaseCost { get; set; }

    /// <summary>
    /// Tahmini teslimat süresi (gün)
    /// </summary>
    public int EstimatedDays { get; set; }
}
