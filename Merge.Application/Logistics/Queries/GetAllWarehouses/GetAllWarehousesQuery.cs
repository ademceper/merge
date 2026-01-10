using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetAllWarehouses;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAllWarehousesQuery(
    bool IncludeInactive = false) : IRequest<IEnumerable<WarehouseDto>>;

