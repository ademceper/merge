using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Commands.ReserveStock;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ReserveStockCommand(
    Guid ProductId,
    Guid WarehouseId,
    int Quantity,
    Guid? OrderId,
    Guid PerformedBy
) : IRequest<bool>;

