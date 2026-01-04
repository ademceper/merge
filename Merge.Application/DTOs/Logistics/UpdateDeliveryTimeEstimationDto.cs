using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

public class UpdateDeliveryTimeEstimationDto
{
    [Range(0, int.MaxValue, ErrorMessage = "Minimum gün 0 veya daha büyük olmalıdır.")]
    public int? MinDays { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Maksimum gün 0 veya daha büyük olmalıdır.")]
    public int? MaxDays { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Ortalama gün 0 veya daha büyük olmalıdır.")]
    public int? AverageDays { get; set; }
    
    public bool? IsActive { get; set; }
    
    /// Typed DTO (Over-posting korumasi)
    public DeliveryTimeSettingsDto? Conditions { get; set; }
}
