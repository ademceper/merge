using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.DTOs.Cart;


public record SaveItemDto(
    [Required(ErrorMessage = "Ürün ID zorunludur")]
    Guid ProductId,

    [Required(ErrorMessage = "Miktar zorunludur")]
    [Range(1, int.MaxValue, ErrorMessage = "Miktar 1'den büyük olmalıdır.")]
    int Quantity,

    [StringLength(1000, ErrorMessage = "Notlar en fazla 1000 karakter olabilir.")]
    string? Notes
);
