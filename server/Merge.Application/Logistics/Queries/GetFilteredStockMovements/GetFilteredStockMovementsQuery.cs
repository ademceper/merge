using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.Domain.Enums;

namespace Merge.Application.Logistics.Queries.GetFilteredStockMovements;

public record GetFilteredStockMovementsQuery(
    Guid? ProductId,
    Guid? WarehouseId,
    StockMovementType? MovementType,
    DateTime? StartDate,
    DateTime? EndDate,
    int Page = 1,
    int PageSize = 20) : IRequest<IEnumerable<StockMovementDto>>;

