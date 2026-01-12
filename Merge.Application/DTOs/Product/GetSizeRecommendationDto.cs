using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Product;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olmalı (ZORUNLU)
public record GetSizeRecommendationDto(
    [Required] Guid ProductId,
    [Required]
    [Range(50, 250, ErrorMessage = "Boy 50 ile 250 cm arasında olmalıdır.")]
    decimal Height,
    
    [Required]
    [Range(20, 300, ErrorMessage = "Kilo 20 ile 300 kg arasında olmalıdır.")]
    decimal Weight,
    
    [Range(50, 200, ErrorMessage = "Göğüs ölçüsü 50 ile 200 cm arasında olmalıdır.")]
    decimal? Chest,
    
    [Range(50, 200, ErrorMessage = "Bel ölçüsü 50 ile 200 cm arasında olmalıdır.")]
    decimal? Waist
);
