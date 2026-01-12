using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// InternationalShipping Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class InternationalShipping : BaseEntity
{
    public Guid OrderId { get; set; }
    public string OriginCountry { get; set; } = string.Empty;
    public string DestinationCountry { get; set; } = string.Empty;
    public string? OriginCity { get; set; }
    public string? DestinationCity { get; set; }
    public string ShippingMethod { get; set; } = string.Empty; // Express, Standard, Economy
    public decimal ShippingCost { get; set; }
    public decimal? CustomsDuty { get; set; }
    public decimal? ImportTax { get; set; }
    public decimal? HandlingFee { get; set; }
    public decimal TotalCost { get; set; }
    public int EstimatedDays { get; set; }
    public string? TrackingNumber { get; set; }
    public string? CustomsDeclarationNumber { get; set; }
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    // Not: ShippingStatus enum'u var ama InternationalShipping için özel değerler gerekebilir
    // Şimdilik ShippingStatus kullanıyoruz, gerekirse yeni enum oluşturulabilir
    public ShippingStatus Status { get; set; } = ShippingStatus.Preparing;
    public DateTime? ShippedAt { get; set; }
    public DateTime? InCustomsAt { get; set; }
    public DateTime? ClearedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    
    // Navigation properties
    public Order Order { get; set; } = null!;
}

