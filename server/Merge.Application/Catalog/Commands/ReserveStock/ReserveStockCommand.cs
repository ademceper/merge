using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Commands.ReserveStock;

public record ReserveStockCommand(
    Guid ProductId,
    Guid WarehouseId,
    int Quantity,
    Guid? OrderId,
    Guid PerformedBy
) : IRequest<bool>;

