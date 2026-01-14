using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record LowStockProductDto(
    Guid ProductId,
    string ProductName,
    string SKU,
    int CurrentStock,
    int MinimumStock,
    int ReorderLevel
);
