using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record CreateSizeGuideEntryDto(
    [Required]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Beden etiketi gereklidir.")]
    string SizeLabel,
    
    [StringLength(50)]
    string? AlternativeLabel,
    
    [Range(0, double.MaxValue, ErrorMessage = "Ölçü değerleri 0 veya daha büyük olmalıdır.")]
    decimal? Chest,
    
    [Range(0, double.MaxValue)]
    decimal? Waist,
    
    [Range(0, double.MaxValue)]
    decimal? Hips,
    
    [Range(0, double.MaxValue)]
    decimal? Inseam,
    
    [Range(0, double.MaxValue)]
    decimal? Shoulder,
    
    [Range(0, double.MaxValue)]
    decimal? Length,
    
    [Range(0, double.MaxValue)]
    decimal? Width,
    
    [Range(0, double.MaxValue)]
    decimal? Height,
    
    [Range(0, double.MaxValue)]
    decimal? Weight,
    
    IReadOnlyDictionary<string, string>? AdditionalMeasurements,
    
    [Range(0, int.MaxValue)]
    int DisplayOrder
);
