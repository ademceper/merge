using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Pay Pre Order Deposit DTO - BOLUM 7.1.5: Records (ZORUNLU)
/// BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
public record PayPreOrderDepositDto(
    [Required(ErrorMessage = "Ön sipariş ID zorunludur")]
    Guid PreOrderId,

    [Required(ErrorMessage = "Tutar zorunludur")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0.01'den büyük olmalıdır.")]
    decimal Amount
);
