using Merge.Domain.Modules.Catalog;
using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Review;

public record RatingDistributionDto(
    int Rating,
    int Count,
    decimal Percentage
);
