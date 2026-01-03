namespace Merge.Domain.Enums;

/// <summary>
/// Email Campaign Status - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum EmailCampaignStatus
{
    Draft,
    Scheduled,
    Sending,
    Sent,
    Paused,
    Cancelled,
    Failed
}

