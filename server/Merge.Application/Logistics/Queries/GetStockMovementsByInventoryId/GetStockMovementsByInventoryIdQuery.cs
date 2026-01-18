using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetStockMovementsByInventoryId;

public record GetStockMovementsByInventoryIdQuery(Guid InventoryId) : IRequest<IEnumerable<StockMovementDto>>;

