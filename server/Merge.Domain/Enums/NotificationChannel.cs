using Merge.Domain.Modules.Notifications;
using Merge.Domain.ValueObjects;
namespace Merge.Domain.Enums;

/// <summary>
/// Notification Channel - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum NotificationChannel
{
    Email,
    Sms,
    Push,
    InApp
}
