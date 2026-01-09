namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record StaticTranslationDto(
    Guid Id,
    string Key,
    string LanguageCode,
    string Value,
    string Category);
