using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Payment;
namespace Merge.Domain.Enums;

/// <summary>
/// Notification Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum NotificationType
{
    Order,
    Payment,
    Shipping,
    Promotion,
    System,
    Security,
    Account,
    Review,
    Support,
    Marketing,
    Warning
}
