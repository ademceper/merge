using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Commands.ReleaseStock;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ReleaseStockCommand(
    Guid ProductId,
    Guid WarehouseId,
    int Quantity,
    Guid? OrderId,
    Guid PerformedBy
) : IRequest<bool>;

