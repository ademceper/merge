namespace Merge.Application.DTOs.Marketing;

/// <summary>
/// Partial update DTO for Email Campaign (PATCH support)
/// All fields are optional for partial updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchEmailCampaignDto
{
    public string? Name { get; init; }
    public string? Subject { get; init; }
    public string? FromName { get; init; }
    public string? FromEmail { get; init; }
    public string? ReplyToEmail { get; init; }
    public Guid? TemplateId { get; init; }
    public string? Content { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public string? TargetSegment { get; init; }
}
