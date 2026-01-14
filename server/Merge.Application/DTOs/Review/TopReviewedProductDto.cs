using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Review;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record TopReviewedProductDto(
    Guid ProductId,
    string ProductName,
    int ReviewCount,
    decimal AverageRating,
    int HelpfulCount
);
