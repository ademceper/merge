using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

public class UpdateEmailCampaignDto
{
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Kampanya adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string? Name { get; set; }
    
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Konu en az 2, en fazla 200 karakter olmalıdır.")]
    public string? Subject { get; set; }
    
    [StringLength(100)]
    public string? FromName { get; set; }
    
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string? FromEmail { get; set; }
    
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string? ReplyToEmail { get; set; }
    
    public Guid? TemplateId { get; set; }
    
    [StringLength(50000)]
    public string? Content { get; set; }
    
    public DateTime? ScheduledAt { get; set; }
    
    [StringLength(100)]
    public string? TargetSegment { get; set; }
}
