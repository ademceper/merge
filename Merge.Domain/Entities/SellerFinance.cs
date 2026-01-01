using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

public class SellerTransaction : BaseEntity
{
    public Guid SellerId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Commission, Payout, Refund, Adjustment
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid? RelatedEntityId { get; set; } // CommissionId, PayoutId, OrderId
    public string? RelatedEntityType { get; set; }
    public FinanceTransactionStatus Status { get; set; } = FinanceTransactionStatus.Completed;

    // Navigation properties
    public User Seller { get; set; } = null!;
}

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

