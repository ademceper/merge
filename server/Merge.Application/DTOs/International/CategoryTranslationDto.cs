namespace Merge.Application.DTOs.International;

public record CategoryTranslationDto(
    Guid Id,
    Guid CategoryId,
    string LanguageCode,
    string Name,
    string Description);
