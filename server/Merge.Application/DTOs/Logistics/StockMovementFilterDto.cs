using Merge.Domain.Enums;

namespace Merge.Application.DTOs.Logistics;

// ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı)
public record StockMovementFilterDto(
    Guid? ProductId = null,
    Guid? WarehouseId = null,
    StockMovementType? MovementType = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int PageSize = 20
);
