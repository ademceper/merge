using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

public class CreateEmailCampaignDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Kampanya adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Konu en az 2, en fazla 200 karakter olmalıdır.")]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Gönderen adı en az 2, en fazla 100 karakter olmalıdır.")]
    public string FromName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string FromEmail { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string ReplyToEmail { get; set; } = string.Empty;
    
    public Guid? TemplateId { get; set; }
    
    [Required]
    [StringLength(50000, MinimumLength = 10, ErrorMessage = "İçerik en az 10, en fazla 50000 karakter olmalıdır.")]
    public string Content { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string Type { get; set; } = "Promotional";
    
    public DateTime? ScheduledAt { get; set; }
    
    [StringLength(100)]
    public string TargetSegment { get; set; } = "All";
    
    public List<string>? Tags { get; set; }
}
