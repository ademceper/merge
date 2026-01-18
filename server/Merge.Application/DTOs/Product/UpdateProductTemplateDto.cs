using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Product;

public record UpdateProductTemplateDto(
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Şablon adı en az 2, en fazla 200 karakter olmalıdır.")]
    string? Name,
    
    [StringLength(2000)]
    string? Description,
    
    Guid? CategoryId,
    
    [StringLength(100)]
    string? Brand,
    
    [StringLength(50)]
    string? DefaultSKUPrefix,
    
    [Range(0, double.MaxValue, ErrorMessage = "Varsayılan fiyat 0 veya daha büyük olmalıdır.")]
    decimal? DefaultPrice,
    
    [Range(0, int.MaxValue, ErrorMessage = "Varsayılan stok miktarı 0 veya daha büyük olmalıdır.")]
    int? DefaultStockQuantity,
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    string? DefaultImageUrl,
    
    IReadOnlyDictionary<string, string>? Specifications,
    
    IReadOnlyDictionary<string, string>? Attributes,
    
    bool? IsActive
);
