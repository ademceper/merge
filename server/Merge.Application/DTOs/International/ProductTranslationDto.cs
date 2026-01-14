namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record ProductTranslationDto(
    Guid Id,
    Guid ProductId,
    string LanguageCode,
    string Name,
    string Description,
    string ShortDescription,
    string MetaTitle,
    string MetaDescription,
    string MetaKeywords);
