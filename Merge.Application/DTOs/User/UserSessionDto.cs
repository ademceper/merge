using Merge.Domain.Modules.Identity;
namespace Merge.Application.DTOs.User;

public class UserSessionDto
{
    public Guid? UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public DateTime SessionStart { get; set; }
    public DateTime SessionEnd { get; set; }
    public int DurationMinutes { get; set; }
    public int ActivitiesCount { get; set; }
    public List<UserActivityLogDto> Activities { get; set; } = new();
}
