using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.DTOs.Payment;

public class VerifyPaymentDto
{
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "İşlem ID gereklidir.")]
    public string TransactionId { get; set; } = string.Empty;
}

