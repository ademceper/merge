using Merge.Domain.Modules.Catalog;
namespace Merge.Application.DTOs.Review;

public record ReviewTrendDto(
    DateTime Date,
    int ReviewCount,
    decimal AverageRating
);
