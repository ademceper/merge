using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Marketing;

public class SendTestEmailDto
{
    [Required(ErrorMessage = "Campaign ID is required")]
    public Guid CampaignId { get; set; }

    [Required(ErrorMessage = "Test email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [StringLength(256, ErrorMessage = "Email address cannot exceed 256 characters")]
    public string TestEmail { get; set; } = string.Empty;
}
