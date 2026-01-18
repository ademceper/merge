using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Queries.GetInventoryById;

public record GetInventoryByIdQuery(
    Guid Id,
    Guid? PerformedBy = null
) : IRequest<InventoryDto?>;

