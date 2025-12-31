namespace Merge.Domain.Entities;

public class NotificationPreference : BaseEntity
{
    public Guid UserId { get; set; }
    public string NotificationType { get; set; } = string.Empty; // Order, Shipping, Payment, Promotion, System, etc.
    public string Channel { get; set; } = string.Empty; // Email, SMS, Push, InApp
    public bool IsEnabled { get; set; } = true;
    public string? CustomSettings { get; set; } // JSON for additional settings
    
    // Navigation properties
    public User User { get; set; } = null!;
}

