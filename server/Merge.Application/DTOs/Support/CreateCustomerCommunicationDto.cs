using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Support;


public record CreateCustomerCommunicationDto
{
    [Required]
    public Guid UserId { get; init; }
    
    [Required]
    [StringLength(100)]
    public string CommunicationType { get; init; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Channel { get; init; } = string.Empty;
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Konu en az 2, en fazla 200 karakter olmalıdır.")]
    public string Subject { get; init; } = string.Empty;
    
    [Required]
    [StringLength(10000, MinimumLength = 1, ErrorMessage = "İçerik en az 1, en fazla 10000 karakter olmalıdır.")]
    public string Content { get; init; } = string.Empty;
    
    [StringLength(50)]
    public string Direction { get; init; } = "Outbound";
    
    public Guid? RelatedEntityId { get; init; }
    
    [StringLength(100)]
    public string? RelatedEntityType { get; init; }
    
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string? RecipientEmail { get; init; }
    
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20)]
    public string? RecipientPhone { get; init; }
    
    /// Typed DTO (Over-posting korumasi)
    public CustomerCommunicationSettingsDto? Metadata { get; init; }
}
