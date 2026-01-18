using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.International;

public record StaticTranslationDto(
    Guid Id,
    string Key,
    string LanguageCode,
    string Value,
    string Category);
