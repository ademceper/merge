using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.DTOs.Payment;

public class ProcessPaymentDto
{
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "İşlem ID gereklidir.")]
    public string TransactionId { get; set; } = string.Empty;

    [StringLength(100)]
    public string PaymentReference { get; set; } = string.Empty;

    public Dictionary<string, string>? Metadata { get; set; }
}

