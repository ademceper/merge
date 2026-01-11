namespace Merge.Domain.Enums;

/// <summary>
/// Verification Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum VerificationType
{
    Manual = 0,
    Automatic = 1,
    Phone = 2,
    Email = 3,
    Document = 4
}
