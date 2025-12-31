namespace Merge.Application.DTOs.Content;

public class LiveChatSessionDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? AgentId { get; set; }
    public string? AgentName { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? GuestName { get; set; }
    public string? GuestEmail { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int MessageCount { get; set; }
    public int UnreadCount { get; set; }
    public string? Department { get; set; }
    public int Priority { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<LiveChatMessageDto> RecentMessages { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
