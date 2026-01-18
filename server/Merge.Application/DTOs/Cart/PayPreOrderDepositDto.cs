using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.DTOs.Cart;


public record PayPreOrderDepositDto(
    [Required(ErrorMessage = "Ön sipariş ID zorunludur")]
    Guid PreOrderId,

    [Required(ErrorMessage = "Tutar zorunludur")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Tutar 0.01'den büyük olmalıdır.")]
    decimal Amount
);
