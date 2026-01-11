using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record UpdateProductBundleDto(
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Paket adı en az 2, en fazla 200 karakter olmalıdır.")]
    string? Name,
    
    [StringLength(2000)]
    string? Description,
    
    [Range(0, double.MaxValue, ErrorMessage = "Paket fiyatı 0 veya daha büyük olmalıdır.")]
    decimal? BundlePrice,
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    string? ImageUrl,
    
    bool? IsActive,
    
    DateTime? StartDate,
    
    DateTime? EndDate
);
