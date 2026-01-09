using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Queries.GetInventoryByProductAndWarehouse;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetInventoryByProductAndWarehouseQuery(
    Guid ProductId,
    Guid WarehouseId,
    Guid? PerformedBy = null
) : IRequest<InventoryDto?>;

