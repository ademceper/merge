using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record UpdateDeliveryTimeEstimationDto(
    [Range(0, int.MaxValue, ErrorMessage = "Minimum gün 0 veya daha büyük olmalıdır.")]
    int? MinDays = null,
    
    [Range(0, int.MaxValue, ErrorMessage = "Maksimum gün 0 veya daha büyük olmalıdır.")]
    int? MaxDays = null,
    
    [Range(0, int.MaxValue, ErrorMessage = "Ortalama gün 0 veya daha büyük olmalıdır.")]
    int? AverageDays = null,
    
    bool? IsActive = null,
    
    DeliveryTimeSettingsDto? Conditions = null // Typed DTO (Over-posting korumasi)
);
