namespace Merge.Application.DTOs.Notification;

public class NotificationTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string TitleTemplate { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
    public string? LinkTemplate { get; set; }
    public bool IsActive { get; set; }
    public Dictionary<string, string>? Variables { get; set; }
    public Dictionary<string, object>? DefaultData { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
