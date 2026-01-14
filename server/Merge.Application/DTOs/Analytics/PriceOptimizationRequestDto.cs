using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record PriceOptimizationRequestDto(
    bool ApplyOptimization = false,
    [Range(0, double.MaxValue, ErrorMessage = "Minimum fiyat 0 veya daha büyük olmalıdır.")] decimal? MinPrice = null,
    [Range(0, double.MaxValue, ErrorMessage = "Maksimum fiyat 0 veya daha büyük olmalıdır.")] decimal? MaxPrice = null,
    [StringLength(100)] string? Strategy = null
);
