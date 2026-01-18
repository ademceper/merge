using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Analytics;

public record RevenueByCategoryDto(
    Guid CategoryId,
    string CategoryName,
    decimal Revenue,
    int OrderCount,
    decimal Percentage
);
