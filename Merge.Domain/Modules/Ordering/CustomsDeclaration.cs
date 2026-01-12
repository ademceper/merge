using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// CustomsDeclaration Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class CustomsDeclaration : BaseEntity
{
    public Guid OrderId { get; set; }
    public string DeclarationNumber { get; set; } = string.Empty;
    public string OriginCountry { get; set; } = string.Empty;
    public string DestinationCountry { get; set; } = string.Empty;
    public decimal TotalValue { get; set; }
    public string Currency { get; set; } = "USD";
    public string? HsCode { get; set; } // Harmonized System code
    public string? Description { get; set; }
    public decimal Weight { get; set; } // in kg
    public int Quantity { get; set; }
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public decimal? CustomsDuty { get; set; }
    public decimal? ImportTax { get; set; }
    public string? Documents { get; set; } // JSON array of document URLs
    
    // Navigation properties
    public Order Order { get; set; } = null!;
}

