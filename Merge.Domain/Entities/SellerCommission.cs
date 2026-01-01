using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Entities;

public enum CommissionStatus
{
    Pending,
    Approved,
    Paid,
    Cancelled
}

public enum PayoutStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled
}

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

public class CommissionTier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal MinSales { get; set; } = 0; // Minimum sales to qualify for this tier
    public decimal MaxSales { get; set; } = decimal.MaxValue;
    public decimal CommissionRate { get; set; } // Percentage
    public decimal PlatformFeeRate { get; set; } = 0; // Percentage
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0; // Higher priority tiers checked first
}

public class SellerCommissionSettings : BaseEntity
{
    public Guid SellerId { get; set; }
    public decimal CustomCommissionRate { get; set; } = 0; // Override default tier rate
    public bool UseCustomRate { get; set; } = false;
    public decimal MinimumPayoutAmount { get; set; } = 100; // Minimum amount to request payout
    public string? PaymentMethod { get; set; } // Bank transfer, PayPal, etc.
    public string? PaymentDetails { get; set; } // Account number, PayPal email, etc.

    // Navigation properties
    public User Seller { get; set; } = null!;
}

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

public class CommissionPayoutItem : BaseEntity
{
    public Guid PayoutId { get; set; }
    public Guid CommissionId { get; set; }

    // Navigation properties
    public CommissionPayout Payout { get; set; } = null!;
    public SellerCommission Commission { get; set; } = null!;
}
