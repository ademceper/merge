namespace Merge.Domain.Enums;

/// <summary>
/// Size Guide Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum SizeGuideType
{
    Standard,      // General size chart
    Detailed,      // Detailed measurements
    Conversion,    // Size conversion chart (US/EU/UK)
    Custom         // Custom sizing system
}

