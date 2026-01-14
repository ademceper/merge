namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
// ⚠️ NOT: Dictionary kullanımı .cursorrules'a göre yasak, ancak mevcut yapıyı koruyoruz
public record BlogAnalyticsDto(
    int TotalPosts,
    int PublishedPosts,
    int DraftPosts,
    int TotalViews,
    int TotalComments,
    Dictionary<string, int> PostsByCategory,
    List<PopularPostDto> PopularPosts
);
