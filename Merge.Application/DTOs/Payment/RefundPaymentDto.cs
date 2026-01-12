using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.DTOs.Payment;

public class RefundPaymentDto
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "İade tutarı 0'dan büyük olmalıdır.")]
    public decimal? Amount { get; set; }
}

