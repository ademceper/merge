namespace Merge.Domain.Enums;

/// <summary>
/// Seller Document Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum SellerDocumentType
{
    Identity = 0,
    Tax = 1,
    Bank = 2,
    License = 3,
    BusinessRegistration = 4,
    Other = 5
}
