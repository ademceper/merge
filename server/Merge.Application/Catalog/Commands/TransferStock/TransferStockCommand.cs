using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Commands.TransferStock;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record TransferStockCommand(
    Guid ProductId,
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    int Quantity,
    string? Notes,
    Guid PerformedBy
) : IRequest<bool>;

