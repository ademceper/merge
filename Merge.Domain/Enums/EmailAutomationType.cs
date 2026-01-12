using Merge.Domain.ValueObjects;
namespace Merge.Domain.Enums;

/// <summary>
/// Email Automation Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum EmailAutomationType
{
    WelcomeSeries,
    AbandonedCart,
    PostPurchase,
    WinBack,
    Birthday,
    ReviewRequest,
    ReorderReminder,
    LowStock,
    BackInStock
}

