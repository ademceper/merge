using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Product;

public record CreateProductBundleDto(
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Paket adı en az 2, en fazla 200 karakter olmalıdır.")]
    string Name,
    
    [StringLength(2000)]
    string Description,
    
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Paket fiyatı 0 veya daha büyük olmalıdır.")]
    decimal BundlePrice,
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    string ImageUrl,
    
    DateTime? StartDate,
    
    DateTime? EndDate,
    
    [Required]
    [MinLength(1, ErrorMessage = "En az bir ürün seçilmelidir.")]
    IReadOnlyList<AddProductToBundleDto> Products
);
