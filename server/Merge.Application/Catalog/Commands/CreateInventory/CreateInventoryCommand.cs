using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Commands.CreateInventory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateInventoryCommand(
    Guid ProductId,
    Guid WarehouseId,
    int Quantity,
    int MinimumStockLevel,
    int MaximumStockLevel,
    decimal UnitCost,
    string? Location,
    Guid PerformedBy
) : IRequest<InventoryDto>;

