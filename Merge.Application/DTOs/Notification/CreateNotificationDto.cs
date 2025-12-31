using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Notification;

public class CreateNotificationDto
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Mesaj en az 1, en fazla 2000 karakter olmalıdır.")]
    public string Message { get; set; } = string.Empty;
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? Link { get; set; }
}
