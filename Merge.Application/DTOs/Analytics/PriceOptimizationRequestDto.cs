using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Analytics;

public class PriceOptimizationRequestDto
{
    public bool ApplyOptimization { get; set; } = false;
    
    [Range(0, double.MaxValue, ErrorMessage = "Minimum fiyat 0 veya daha büyük olmalıdır.")]
    public decimal? MinPrice { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Maksimum fiyat 0 veya daha büyük olmalıdır.")]
    public decimal? MaxPrice { get; set; }
    
    [StringLength(100)]
    public string? Strategy { get; set; }
}
