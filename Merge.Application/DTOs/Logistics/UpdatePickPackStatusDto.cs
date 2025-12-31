using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

public class UpdatePickPackStatusDto
{
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;
    
    [StringLength(2000)]
    public string? Notes { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Ağırlık 0 veya daha büyük olmalıdır.")]
    public decimal? Weight { get; set; }
    
    [StringLength(100)]
    public string? Dimensions { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Paket sayısı en az 1 olmalıdır.")]
    public int? PackageCount { get; set; }
}
