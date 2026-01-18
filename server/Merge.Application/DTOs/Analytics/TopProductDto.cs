using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Analytics;

public record TopProductDto(
    Guid ProductId,
    string ProductName,
    string SKU,
    int UnitsSold,
    decimal Revenue,
    decimal AveragePrice
);
