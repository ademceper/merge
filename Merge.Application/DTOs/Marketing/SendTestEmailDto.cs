using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Send Test Email DTO - BOLUM 1.0: DTO Dosya Organizasyonu (ZORUNLU)
/// </summary>
public record SendTestEmailDto
{
    [Required(ErrorMessage = "Campaign ID is required")]
    public Guid CampaignId { get; init; }

    [Required(ErrorMessage = "Test email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [StringLength(256, ErrorMessage = "Email address cannot exceed 256 characters")]
    public string TestEmail { get; init; } = string.Empty;
}
