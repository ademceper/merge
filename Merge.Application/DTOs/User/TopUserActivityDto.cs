namespace Merge.Application.DTOs.User;

public class TopUserActivityDto
{
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public int ActivityCount { get; set; }
    public DateTime LastActivity { get; set; }
}
