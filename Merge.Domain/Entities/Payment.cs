using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // CreditCard, BankTransfer, CashOnDelivery
    public string PaymentProvider { get; set; } = string.Empty; // iyzico, paytr, stripe
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed, Refunded
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; } // Ödeme sağlayıcıdan gelen ID
    public string? PaymentReference { get; set; } // Ödeme referans numarası
    public DateTime? PaidAt { get; set; }
    public string? FailureReason { get; set; }
    public string? Metadata { get; set; } // JSON formatında ek bilgiler

    // ✅ CONCURRENCY: Eşzamanlı ödeme işlemlerini önlemek için
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Order Order { get; set; } = null!;
}

