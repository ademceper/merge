using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Commands.AdjustStock;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AdjustStockCommand(
    Guid InventoryId,
    int QuantityChange,
    string? Notes,
    Guid PerformedBy
) : IRequest<InventoryDto>;

