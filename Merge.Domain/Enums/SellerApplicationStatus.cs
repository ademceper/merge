namespace Merge.Domain.Enums;

/// <summary>
/// Seller Application Status - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum SellerApplicationStatus
{
    Pending,
    Submitted,
    UnderReview,
    Approved,
    Rejected,
    RequiresMoreInfo
}

