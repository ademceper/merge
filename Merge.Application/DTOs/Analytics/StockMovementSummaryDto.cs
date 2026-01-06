namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record StockMovementSummaryDto(
    string MovementType,
    int Count,
    int TotalQuantity,
    DateTime Date
);
