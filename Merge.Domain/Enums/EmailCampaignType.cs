namespace Merge.Domain.Enums;

/// <summary>
/// Email Campaign Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum EmailCampaignType
{
    Promotional,
    Transactional,
    Newsletter,
    Announcement,
    AbandonedCart,
    WelcomeSeries,
    ProductRecommendation,
    WinBack,
    Seasonal,
    Survey
}

