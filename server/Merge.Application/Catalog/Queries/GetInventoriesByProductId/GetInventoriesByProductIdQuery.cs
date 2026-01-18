using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Queries.GetInventoriesByProductId;

public record GetInventoriesByProductIdQuery(
    Guid ProductId,
    Guid? PerformedBy = null
) : IRequest<IEnumerable<InventoryDto>>;

