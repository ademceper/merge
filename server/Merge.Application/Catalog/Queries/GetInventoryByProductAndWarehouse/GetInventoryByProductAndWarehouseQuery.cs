using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Queries.GetInventoryByProductAndWarehouse;

public record GetInventoryByProductAndWarehouseQuery(
    Guid ProductId,
    Guid WarehouseId,
    Guid? PerformedBy = null
) : IRequest<InventoryDto?>;

