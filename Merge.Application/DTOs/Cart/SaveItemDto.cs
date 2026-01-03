using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Save Item DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
public class SaveItemDto
{
    [Required(ErrorMessage = "Ürün ID zorunludur")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "Miktar zorunludur")]
    [Range(1, int.MaxValue, ErrorMessage = "Miktar 1'den büyük olmalıdır.")]
    public int Quantity { get; set; } = 1;

    [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir.")]
    public string? Notes { get; set; }
}
