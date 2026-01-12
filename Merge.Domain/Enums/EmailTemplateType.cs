using Merge.Domain.ValueObjects;
namespace Merge.Domain.Enums;

/// <summary>
/// Email Template Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum EmailTemplateType
{
    Custom,
    Welcome,
    OrderConfirmation,
    ShippingUpdate,
    DeliveryConfirmation,
    PasswordReset,
    AccountActivation,
    Newsletter,
    Promotional,
    AbandonedCart,
    ProductRecommendation,
    ReviewRequest,
    WinBack,
    Receipt
}

