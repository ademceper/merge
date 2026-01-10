using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record CompletePackingDto(
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Ağırlık 0 veya daha büyük olmalıdır.")]
    decimal Weight,
    
    [StringLength(100)]
    string? Dimensions = null,
    
    [Range(1, int.MaxValue, ErrorMessage = "Paket sayısı en az 1 olmalıdır.")]
    int PackageCount = 1
);

