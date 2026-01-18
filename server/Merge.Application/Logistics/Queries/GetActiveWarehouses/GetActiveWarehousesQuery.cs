using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetActiveWarehouses;

public record GetActiveWarehousesQuery() : IRequest<IEnumerable<WarehouseDto>>;

