using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Queries.GetAvailableStock;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAvailableStockQuery(
    Guid ProductId,
    Guid? WarehouseId = null,
    Guid? PerformedBy = null
) : IRequest<AvailableStockDto>;

