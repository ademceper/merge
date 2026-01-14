using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetWarehouseByCode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetWarehouseByCodeQuery(string Code) : IRequest<WarehouseDto?>;

