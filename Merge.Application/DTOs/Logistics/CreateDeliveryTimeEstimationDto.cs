using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

public class CreateDeliveryTimeEstimationDto
{
    public Guid? ProductId { get; set; }
    
    public Guid? CategoryId { get; set; }
    
    public Guid? WarehouseId { get; set; }
    
    public Guid? ShippingProviderId { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(100)]
    public string? Country { get; set; }
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Minimum gün 0 veya daha büyük olmalıdır.")]
    public int MinDays { get; set; }
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Maksimum gün 0 veya daha büyük olmalıdır.")]
    public int MaxDays { get; set; }
    
    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Ortalama gün 0 veya daha büyük olmalıdır.")]
    public int AverageDays { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public Dictionary<string, object>? Conditions { get; set; }
}
