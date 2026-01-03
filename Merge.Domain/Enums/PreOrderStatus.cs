namespace Merge.Domain.Enums;

/// <summary>
/// Pre Order Status - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum PreOrderStatus
{
    Pending,
    DepositPaid,
    Confirmed,
    ReadyToShip,
    Converted,
    Cancelled,
    Expired
}

