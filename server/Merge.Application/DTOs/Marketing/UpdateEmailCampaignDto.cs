using System.ComponentModel.DataAnnotations;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Marketing;


public record UpdateEmailCampaignDto
{
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Kampanya adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string? Name { get; init; }
    
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Konu en az 2, en fazla 200 karakter olmalıdır.")]
    public string? Subject { get; init; }
    
    [StringLength(100)]
    public string? FromName { get; init; }
    
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string? FromEmail { get; init; }
    
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string? ReplyToEmail { get; init; }
    
    public Guid? TemplateId { get; init; }
    
    [StringLength(50000)]
    public string? Content { get; init; }
    
    public DateTime? ScheduledAt { get; init; }
    
    [StringLength(100)]
    public string? TargetSegment { get; init; }
}
