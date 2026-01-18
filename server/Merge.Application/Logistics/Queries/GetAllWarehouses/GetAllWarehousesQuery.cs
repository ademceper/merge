using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetAllWarehouses;

public record GetAllWarehousesQuery(
    bool IncludeInactive = false) : IRequest<IEnumerable<WarehouseDto>>;

