using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.Domain.Enums;

namespace Merge.Application.Logistics.Queries.GetFilteredStockMovements;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 6.3: Unbounded Query Koruması - Güvenlik için limit ekle
public record GetFilteredStockMovementsQuery(
    Guid? ProductId,
    Guid? WarehouseId,
    StockMovementType? MovementType,
    DateTime? StartDate,
    DateTime? EndDate,
    int Page = 1,
    int PageSize = 20) : IRequest<IEnumerable<StockMovementDto>>;

