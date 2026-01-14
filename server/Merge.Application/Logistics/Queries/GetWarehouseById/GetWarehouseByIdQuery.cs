using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetWarehouseById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetWarehouseByIdQuery(Guid Id) : IRequest<WarehouseDto?>;

