using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Review;

public record TopReviewedProductDto(
    Guid ProductId,
    string ProductName,
    int ReviewCount,
    decimal AverageRating,
    int HelpfulCount
);
