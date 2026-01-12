using System.ComponentModel.DataAnnotations;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Create Email Automation DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record CreateEmailAutomationDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Otomasyon adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; init; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; init; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Type { get; init; } = string.Empty;
    
    [Required]
    public Guid TemplateId { get; init; }
    
    [Range(0, int.MaxValue, ErrorMessage = "Gecikme saati 0 veya daha büyük olmalıdır.")]
    public int DelayHours { get; init; } = 0;
    
    /// Typed DTO (Over-posting korumasi)
    public EmailAutomationSettingsDto? TriggerConditions { get; init; }
}
