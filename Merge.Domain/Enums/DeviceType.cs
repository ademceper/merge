namespace Merge.Domain.Enums;

/// <summary>
/// Device Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.2: Enum kullanımı (string DeviceType YASAK)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum DeviceType
{
    Mobile,
    Desktop,
    Tablet,
    Other
}
