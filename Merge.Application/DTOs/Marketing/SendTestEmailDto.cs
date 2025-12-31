namespace Merge.Application.DTOs.Marketing;

public class SendTestEmailDto
{
    public Guid CampaignId { get; set; }
    public string TestEmail { get; set; } = string.Empty;
}
