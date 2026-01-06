namespace Merge.Application.DTOs.Review;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record ReviewerStatsDto(
    Guid UserId,
    string UserName,
    int ReviewCount,
    decimal AverageRating,
    int HelpfulVotes
);
