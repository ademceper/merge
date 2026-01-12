using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record CreateProductFromTemplateDto(
    [Required] Guid TemplateId,
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Ürün adı en az 2, en fazla 200 karakter olmalıdır.")]
    string ProductName,
    
    [StringLength(2000)]
    string? Description,
    
    [StringLength(100)]
    string? SKU,
    
    [Range(0, double.MaxValue, ErrorMessage = "Fiyat 0 veya daha büyük olmalıdır.")]
    decimal? Price,
    
    [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0 veya daha büyük olmalıdır.")]
    int? StockQuantity,
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    string? ImageUrl,
    
    Guid? SellerId,
    
    Guid? StoreId,
    
    IReadOnlyDictionary<string, string>? AdditionalSpecifications,
    
    IReadOnlyDictionary<string, string>? AdditionalAttributes,
    
    IReadOnlyList<string>? ImageUrls,
    
    [Range(0, double.MaxValue, ErrorMessage = "İndirimli fiyat 0 veya daha büyük olmalıdır.")]
    decimal? DiscountPrice
);
