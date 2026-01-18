using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Queries.GetLowStockAlerts;

public record GetLowStockAlertsQuery(
    Guid PerformedBy,
    Guid? WarehouseId = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<LowStockAlertDto>>;

