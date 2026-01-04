namespace Merge.Application.DTOs.Organization;

public class TeamDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? TeamLeadId { get; set; }
    public string? TeamLeadName { get; set; }
    public bool IsActive { get; set; }
    /// <summary>
    /// Takim ayarlari - Typed DTO (Over-posting korumasi)
    /// </summary>
    public TeamSettingsDto? Settings { get; set; }
    public int MemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
