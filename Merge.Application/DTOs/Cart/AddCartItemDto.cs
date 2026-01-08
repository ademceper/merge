using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Add Cart Item DTO - BOLUM 7.1.5: Records (ZORUNLU)
/// BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
public record AddCartItemDto(
    [Required(ErrorMessage = "Ürün ID zorunludur")]
    Guid ProductId,

    [Required(ErrorMessage = "Miktar zorunludur")]
    [Range(1, int.MaxValue, ErrorMessage = "Miktar 1'den büyük olmalıdır.")]
    int Quantity
);

