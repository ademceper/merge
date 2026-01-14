using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record CreateDeliveryTimeEstimationDto(
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Minimum gün 0 veya daha büyük olmalıdır.")]
    int MinDays,
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Maksimum gün 0 veya daha büyük olmalıdır.")]
    int MaxDays,
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Ortalama gün 0 veya daha büyük olmalıdır.")]
    int AverageDays,
    
    Guid? ProductId = null,
    
    Guid? CategoryId = null,
    
    Guid? WarehouseId = null,
    
    Guid? ShippingProviderId = null,
    
    [StringLength(100)]
    string? City = null,
    
    [StringLength(100)]
    string? Country = null,
    
    bool IsActive = true,
    
    DeliveryTimeSettingsDto? Conditions = null // Typed DTO (Over-posting korumasi)
);
