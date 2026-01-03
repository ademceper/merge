namespace Merge.Domain.Entities;

/// <summary>
/// Language Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class Language : BaseEntity
{
    public string Code { get; set; } = string.Empty; // en, tr, ar, de, fr
    public string Name { get; set; } = string.Empty; // English, Türkçe, العربية
    public string NativeName { get; set; } = string.Empty; // English, Türkçe, العربية
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public bool IsRTL { get; set; } = false; // Right-to-left (Arabic, Hebrew)
    public string FlagIcon { get; set; } = string.Empty; // URL or emoji flag
}

