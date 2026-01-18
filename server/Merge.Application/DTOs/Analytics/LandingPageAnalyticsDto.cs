namespace Merge.Application.DTOs.Analytics;

// ⚠️ NOT: Dictionary kullanımı .cursorrules'a göre yasak, ancak mevcut yapıyı koruyoruz
public record LandingPageAnalyticsDto(
    Guid LandingPageId,
    string LandingPageName,
    int TotalViews,
    int TotalConversions,
    decimal ConversionRate,
    Dictionary<string, int> ViewsByDate,
    Dictionary<string, int> ConversionsByDate,
    List<LandingPageVariantDto> Variants
);
