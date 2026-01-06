namespace Merge.Application.DTOs.Review;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record ReviewTrendDto(
    DateTime Date,
    int ReviewCount,
    decimal AverageRating
);
