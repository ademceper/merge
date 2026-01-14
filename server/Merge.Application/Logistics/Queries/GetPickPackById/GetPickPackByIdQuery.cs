using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetPickPackById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetPickPackByIdQuery(Guid Id) : IRequest<PickPackDto?>;

