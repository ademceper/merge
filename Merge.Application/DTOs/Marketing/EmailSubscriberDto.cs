namespace Merge.Application.DTOs.Marketing;

public class EmailSubscriberDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsSubscribed { get; set; }
    public DateTime SubscribedAt { get; set; }
    public DateTime? UnsubscribedAt { get; set; }
    public string? Source { get; set; }
    public int EmailsSent { get; set; }
    public int EmailsOpened { get; set; }
    public int EmailsClicked { get; set; }
    public List<string> Tags { get; set; } = new();
}
