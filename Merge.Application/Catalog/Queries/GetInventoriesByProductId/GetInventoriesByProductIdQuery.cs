using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Queries.GetInventoriesByProductId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetInventoriesByProductIdQuery(
    Guid ProductId,
    Guid? PerformedBy = null
) : IRequest<IEnumerable<InventoryDto>>;

