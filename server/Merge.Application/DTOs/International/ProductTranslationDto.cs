namespace Merge.Application.DTOs.International;

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
