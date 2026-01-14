namespace Merge.Domain.Enums;

/// <summary>
/// Subscription Plan Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// BOLUM 1.2: Enum kullanımı (string YASAK)
/// </summary>
public enum SubscriptionPlanType
{
    Monthly = 0,
    Quarterly = 1,
    Yearly = 2,
    Lifetime = 3
}

