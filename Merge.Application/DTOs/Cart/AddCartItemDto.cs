using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Add Cart Item DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
public class AddCartItemDto
{
    [Required(ErrorMessage = "Ürün ID zorunludur")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Miktar zorunludur")]
    [Range(1, int.MaxValue, ErrorMessage = "Miktar 1'den büyük olmalıdır.")]
    public int Quantity { get; set; }
}

