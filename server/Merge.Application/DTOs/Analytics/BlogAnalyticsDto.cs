namespace Merge.Application.DTOs.Analytics;

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
