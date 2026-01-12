using System.ComponentModel.DataAnnotations;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Create Email Campaign DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record CreateEmailCampaignDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Kampanya adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; init; } = string.Empty;
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Konu en az 2, en fazla 200 karakter olmalıdır.")]
    public string Subject { get; init; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Gönderen adı en az 2, en fazla 100 karakter olmalıdır.")]
    public string FromName { get; init; } = string.Empty;
    
    [Required]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string FromEmail { get; init; } = string.Empty;
    
    [Required]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string ReplyToEmail { get; init; } = string.Empty;
    
    public Guid? TemplateId { get; init; }
    
    [Required]
    [StringLength(50000, MinimumLength = 10, ErrorMessage = "İçerik en az 10, en fazla 50000 karakter olmalıdır.")]
    public string Content { get; init; } = string.Empty;
    
    [StringLength(50)]
    public string Type { get; init; } = "Promotional";
    
    public DateTime? ScheduledAt { get; init; }
    
    [StringLength(100)]
    public string TargetSegment { get; init; } = "All";
    
    public List<string>? Tags { get; init; }
}
