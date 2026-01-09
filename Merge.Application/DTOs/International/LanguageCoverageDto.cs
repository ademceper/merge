namespace Merge.Application.DTOs.International;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record LanguageCoverageDto(
    string LanguageCode,
    string LanguageName,
    int ProductsTranslated,
    int TotalProducts,
    decimal CoveragePercentage);
