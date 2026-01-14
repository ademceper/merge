namespace Merge.Domain.Enums;

/// <summary>
/// Seller Transaction Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum SellerTransactionType
{
    Commission = 0,
    Payout = 1,
    Refund = 2,
    Adjustment = 3,
    Fee = 4,
    Chargeback = 5
}
