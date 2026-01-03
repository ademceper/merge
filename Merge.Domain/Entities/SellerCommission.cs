using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// SellerCommission Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class SellerCommission : BaseEntity
{
    public Guid SellerId { get; set; }
    public Guid OrderId { get; set; }
    public Guid OrderItemId { get; set; }
    public decimal OrderAmount { get; set; }
    public decimal CommissionRate { get; set; } // Percentage
    public decimal CommissionAmount { get; set; }
    public decimal PlatformFee { get; set; } = 0;
    public decimal NetAmount { get; set; } // CommissionAmount - PlatformFee
    public CommissionStatus Status { get; set; } = CommissionStatus.Pending;
    public DateTime? ApprovedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? PaymentReference { get; set; }

    // ✅ CONCURRENCY: Eşzamanlı güncellemeleri önlemek için
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public User Seller { get; set; } = null!;
    public Order Order { get; set; } = null!;
    public OrderItem OrderItem { get; set; } = null!;
}

