using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.DTOs.Payment;

public class CreatePaymentDto
{
    [Required]
    public Guid OrderId { get; set; }

    [Required]
    [StringLength(50)]
    public string PaymentMethod { get; set; } = string.Empty; // CreditCard, BankTransfer, CashOnDelivery

    [Required]
    [StringLength(50)]
    public string PaymentProvider { get; set; } = string.Empty; // iyzico, paytr, stripe

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Ödeme tutarı 0'dan büyük olmalıdır.")]
    public decimal Amount { get; set; }
}

