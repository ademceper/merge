namespace Merge.Application.DTOs.International;

public record TranslationStatsDto(
    int TotalLanguages,
    int ActiveLanguages,
    string DefaultLanguage,
    IReadOnlyList<LanguageCoverageDto> LanguageCoverage);
