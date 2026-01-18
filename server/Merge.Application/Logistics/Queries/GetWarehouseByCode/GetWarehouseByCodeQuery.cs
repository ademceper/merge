using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetWarehouseByCode;

public record GetWarehouseByCodeQuery(string Code) : IRequest<WarehouseDto?>;

