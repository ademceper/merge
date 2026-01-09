namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record TranslationStatsDto(
    int TotalLanguages,
    int ActiveLanguages,
    string DefaultLanguage,
    IReadOnlyList<LanguageCoverageDto> LanguageCoverage);
