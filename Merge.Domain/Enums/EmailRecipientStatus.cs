using Merge.Domain.ValueObjects;
namespace Merge.Domain.Enums;

/// <summary>
/// Email Recipient Status - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum EmailRecipientStatus
{
    Pending,
    Sent,
    Delivered,
    Opened,
    Clicked,
    Bounced,
    Failed,
    Unsubscribed
}

