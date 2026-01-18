using MediatR;
using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;

namespace Merge.Application.Logistics.Queries.GetStockMovementsByWarehouseId;

public record GetStockMovementsByWarehouseIdQuery(
    Guid WarehouseId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<StockMovementDto>>;

