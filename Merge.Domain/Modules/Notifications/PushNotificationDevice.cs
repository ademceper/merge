using Merge.Domain.SharedKernel;
using Merge.Domain.Modules.Identity;
namespace Merge.Domain.Modules.Notifications;

/// <summary>
/// PushNotificationDevice Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class PushNotificationDevice : BaseEntity
{
    public Guid UserId { get; set; }
    public string DeviceToken { get; set; } = string.Empty; // FCM token or APNS token
    public string Platform { get; set; } = string.Empty; // iOS, Android, Web
    public string? DeviceId { get; set; } // Unique device identifier
    public string? DeviceModel { get; set; }
    public string? AppVersion { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastUsedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
}

