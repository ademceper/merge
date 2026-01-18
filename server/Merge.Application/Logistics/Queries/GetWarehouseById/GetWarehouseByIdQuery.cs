using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetWarehouseById;

public record GetWarehouseByIdQuery(Guid Id) : IRequest<WarehouseDto?>;

