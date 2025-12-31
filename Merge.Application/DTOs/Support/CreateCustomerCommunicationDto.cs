using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;

public class CreateCustomerCommunicationDto
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string CommunicationType { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Channel { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Konu en az 2, en fazla 200 karakter olmalıdır.")]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    [StringLength(10000, MinimumLength = 1, ErrorMessage = "İçerik en az 1, en fazla 10000 karakter olmalıdır.")]
    public string Content { get; set; } = string.Empty;
    
    [StringLength(50)]
    public string Direction { get; set; } = "Outbound";
    
    public Guid? RelatedEntityId { get; set; }
    
    [StringLength(100)]
    public string? RelatedEntityType { get; set; }
    
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string? RecipientEmail { get; set; }
    
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20)]
    public string? RecipientPhone { get; set; }
    
    public Dictionary<string, object>? Metadata { get; set; }
}
