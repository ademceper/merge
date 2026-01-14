namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record PopularPostDto(
    Guid PostId,
    string Title,
    int ViewCount,
    int CommentCount
);
