using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record UpdatePickPackStatusDto(
    [Required]
    [StringLength(50)]
    string Status,
    
    [StringLength(2000)]
    string? Notes = null,
    
    [Range(0, double.MaxValue, ErrorMessage = "Ağırlık 0 veya daha büyük olmalıdır.")]
    decimal? Weight = null,
    
    [StringLength(100)]
    string? Dimensions = null,
    
    [Range(1, int.MaxValue, ErrorMessage = "Paket sayısı en az 1 olmalıdır.")]
    int? PackageCount = null
);
