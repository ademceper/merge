using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// SellerInvoice Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SellerInvoice : BaseEntity
{
    public Guid SellerId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty; // Auto-generated: INV-YYYYMM-XXXXXX
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal TotalCommissions { get; set; }
    public decimal TotalPayouts { get; set; }
    public decimal PlatformFees { get; set; }
    public decimal NetAmount { get; set; }
    public SellerInvoiceStatus Status { get; set; } = SellerInvoiceStatus.Draft;
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }
    public string? InvoiceData { get; set; } // JSON for invoice items
    
    // Navigation properties
    public User Seller { get; set; } = null!;
}

