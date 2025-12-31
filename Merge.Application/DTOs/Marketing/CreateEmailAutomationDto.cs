using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

public class CreateEmailAutomationDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Otomasyon adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    public Guid TemplateId { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Gecikme saati 0 veya daha büyük olmalıdır.")]
    public int DelayHours { get; set; } = 0;
    
    public Dictionary<string, object>? TriggerConditions { get; set; }
}
