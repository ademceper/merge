using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Create Pre Order DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
public class CreatePreOrderDto
{
    [Required(ErrorMessage = "Ürün ID zorunludur")]
    public Guid ProductId { get; set; }
    
    [Required(ErrorMessage = "Miktar zorunludur")]
    [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır.")]
    public int Quantity { get; set; } = 1;
    
    [StringLength(500, ErrorMessage = "Varyant seçenekleri en fazla 500 karakter olabilir.")]
    public string? VariantOptions { get; set; }
    
    [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir.")]
    public string? Notes { get; set; }
}
