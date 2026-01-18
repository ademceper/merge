using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Analytics;

public record LowStockProductDto(
    Guid ProductId,
    string ProductName,
    string SKU,
    int CurrentStock,
    int MinimumStock,
    int ReorderLevel
);
