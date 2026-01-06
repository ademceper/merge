namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
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
