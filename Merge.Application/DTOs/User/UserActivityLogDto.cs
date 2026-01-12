using Merge.Domain.Enums;
using Merge.Domain.Modules.Identity;
namespace Merge.Application.DTOs.User;

public class UserActivityLogDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid? EntityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Browser { get; set; } = string.Empty;
    public string OS { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int DurationMs { get; set; }
    public bool WasSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
}
