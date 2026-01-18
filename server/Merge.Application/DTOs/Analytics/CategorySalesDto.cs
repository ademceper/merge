namespace Merge.Application.DTOs.Analytics;

public record CategorySalesDto(
    Guid CategoryId,
    string CategoryName,
    decimal Revenue,
    int OrderCount,
    int ProductsSold
);
