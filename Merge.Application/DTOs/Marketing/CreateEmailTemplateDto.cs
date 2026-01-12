using System.ComponentModel.DataAnnotations;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Create Email Template DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record CreateEmailTemplateDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Şablon adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; init; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; init; } = string.Empty;
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Konu en az 2, en fazla 200 karakter olmalıdır.")]
    public string Subject { get; init; } = string.Empty;
    
    [Required]
    [StringLength(100000, MinimumLength = 10, ErrorMessage = "HTML içerik en az 10, en fazla 100000 karakter olmalıdır.")]
    public string HtmlContent { get; init; } = string.Empty;
    
    [StringLength(50000)]
    public string TextContent { get; init; } = string.Empty;
    
    [StringLength(50)]
    public string Type { get; init; } = "Custom";
    
    public List<string>? Variables { get; init; }
    
    [StringLength(500)]
    public string? Thumbnail { get; init; }
}
