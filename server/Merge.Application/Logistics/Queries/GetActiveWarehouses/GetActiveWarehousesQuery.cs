using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetActiveWarehouses;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetActiveWarehousesQuery() : IRequest<IEnumerable<WarehouseDto>>;

