using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.DTOs.Payment;

/// <summary>
/// Payment DTO - BOLUM 1.2: Enum kullanımı (string Status YASAK)
/// </summary>
public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentProvider { get; set; } = string.Empty;
    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentReference { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

