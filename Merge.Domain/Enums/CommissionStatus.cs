namespace Merge.Domain.Enums;

/// <summary>
/// Commission Status - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum CommissionStatus
{
    Pending,
    Approved,
    Paid,
    Cancelled
}

