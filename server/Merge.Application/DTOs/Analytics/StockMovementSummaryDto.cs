namespace Merge.Application.DTOs.Analytics;

public record StockMovementSummaryDto(
    string MovementType,
    int Count,
    int TotalQuantity,
    DateTime Date
);
