namespace Merge.Application.DTOs.Security;

public class RecentCriticalEventDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
