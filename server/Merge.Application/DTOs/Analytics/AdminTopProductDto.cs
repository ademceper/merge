namespace Merge.Application.DTOs.Analytics;

public record AdminTopProductDto(
    Guid ProductId,
    string ProductName,
    string ImageUrl,
    int TotalSold,
    decimal TotalRevenue
);
