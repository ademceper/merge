namespace Merge.Application.DTOs.International;

public record LanguageCoverageDto(
    string LanguageCode,
    string LanguageName,
    int ProductsTranslated,
    int TotalProducts,
    decimal CoveragePercentage);
