namespace Merge.Application.DTOs.Review;

public class TrustBadgeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public string BadgeType { get; set; } = string.Empty;
    /// Typed DTO (Over-posting korumasi)
    public TrustBadgeSettingsDto? Criteria { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string? Color { get; set; }
    public DateTime CreatedAt { get; set; }
}
