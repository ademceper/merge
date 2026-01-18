using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetStockMovementById;

public record GetStockMovementByIdQuery(Guid Id) : IRequest<StockMovementDto?>;

