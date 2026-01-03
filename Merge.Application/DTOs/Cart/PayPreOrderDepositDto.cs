using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Pay Pre Order Deposit DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
public class PayPreOrderDepositDto
{
    [Required(ErrorMessage = "Ön sipariş ID zorunludur")]
    public Guid PreOrderId { get; set; }

    [Required(ErrorMessage = "Tutar zorunludur")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0.01'den büyük olmalıdır.")]
    public decimal Amount { get; set; }
}
