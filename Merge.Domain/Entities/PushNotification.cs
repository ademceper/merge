using Merge.Domain.Enums;

namespace Merge.Domain.Entities;

/// <summary>
/// PushNotification Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class PushNotification : BaseEntity
{
    public Guid? UserId { get; set; } // Null for broadcast notifications
    public Guid? DeviceId { get; set; } // Specific device, null for all user devices
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Data { get; set; } // JSON data payload
    // ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public CommunicationStatus Status { get; set; } = CommunicationStatus.Pending;
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string NotificationType { get; set; } = string.Empty; // Order, Shipping, Promotion, etc.
    
    // Navigation properties
    public User? User { get; set; }
    public PushNotificationDevice? Device { get; set; }
}

