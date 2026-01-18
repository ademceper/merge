using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Queries.GetInventoriesByWarehouseId;

public record GetInventoriesByWarehouseIdQuery(
    Guid WarehouseId,
    Guid PerformedBy,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<InventoryDto>>;

