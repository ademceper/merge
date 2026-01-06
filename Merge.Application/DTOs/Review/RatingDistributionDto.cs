namespace Merge.Application.DTOs.Review;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record RatingDistributionDto(
    int Rating,
    int Count,
    decimal Percentage
);
