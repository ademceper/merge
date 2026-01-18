using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Queries.GetAvailableStock;

public record GetAvailableStockQuery(
    Guid ProductId,
    Guid? WarehouseId = null,
    Guid? PerformedBy = null
) : IRequest<AvailableStockDto>;

