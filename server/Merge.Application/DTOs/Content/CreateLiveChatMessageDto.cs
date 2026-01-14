using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Content;

public class CreateLiveChatMessageDto
{
    [Required]
    public Guid SessionId { get; set; }
    
    [Required]
    // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - DTO validation matches SupportSettings.MaxLiveChatMessageLength=10000
    [StringLength(10000, MinimumLength = 1, ErrorMessage = "Mesaj içeriği en az 1, en fazla 10000 karakter olmalıdır.")]
    public string Content { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string MessageType { get; set; } = "Text";
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? FileUrl { get; set; }
    
    [StringLength(200)]
    public string? FileName { get; set; }
    
    public bool IsInternal { get; set; } = false;
}
