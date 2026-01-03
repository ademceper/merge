using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// CommissionPayout Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class CommissionPayout : BaseEntity
{
    public Guid SellerId { get; set; }
    public string PayoutNumber { get; set; } = string.Empty; // Auto-generated: PAY-XXXXXX
    public decimal TotalAmount { get; set; }
    public decimal TransactionFee { get; set; } = 0;
    public decimal NetAmount { get; set; }
    public PayoutStatus Status { get; set; } = PayoutStatus.Pending;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? PaymentDetails { get; set; }
    public string? TransactionReference { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public User Seller { get; set; } = null!;
    public ICollection<CommissionPayoutItem> Items { get; set; } = new List<CommissionPayoutItem>();
}

