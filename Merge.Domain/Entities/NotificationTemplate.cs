namespace Merge.Domain.Entities;

public class NotificationTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Order, Payment, Shipping, Promotion, System
    public string TitleTemplate { get; set; } = string.Empty; // Template with variables like {OrderNumber}
    public string MessageTemplate { get; set; } = string.Empty; // Template with variables
    public string? LinkTemplate { get; set; } // Optional link template
    public bool IsActive { get; set; } = true;
    public string? Variables { get; set; } // JSON formatında kullanılabilir değişkenler listesi
    public string? DefaultData { get; set; } // JSON formatında varsayılan data
}

