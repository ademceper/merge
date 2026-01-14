namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record CategoryTranslationDto(
    Guid Id,
    Guid CategoryId,
    string LanguageCode,
    string Name,
    string Description);
