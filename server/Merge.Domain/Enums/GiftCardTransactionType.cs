namespace Merge.Domain.Enums;

/// <summary>
/// Gift Card Transaction Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.2: Enum kullanımı (string TransactionType YASAK)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum GiftCardTransactionType
{
    Purchase, // Gift card purchased
    Redeem, // Gift card redeemed
    Refund // Gift card refunded
}
