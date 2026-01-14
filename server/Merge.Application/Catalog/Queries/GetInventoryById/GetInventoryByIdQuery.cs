using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Queries.GetInventoryById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetInventoryByIdQuery(
    Guid Id,
    Guid? PerformedBy = null
) : IRequest<InventoryDto?>;

