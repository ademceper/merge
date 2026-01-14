using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Create Pre Order DTO - BOLUM 7.1.5: Records (ZORUNLU)
/// BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
public record CreatePreOrderDto(
    [Required(ErrorMessage = "Ürün ID zorunludur")]
    Guid ProductId,
    
    [Required(ErrorMessage = "Miktar zorunludur")]
    [Range(1, int.MaxValue, ErrorMessage = "Miktar en az 1 olmalıdır.")]
    int Quantity,
    
    [StringLength(500, ErrorMessage = "Varyant seçenekleri en fazla 500 karakter olabilir.")]
    string? VariantOptions,
    
    [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir.")]
    string? Notes
);
