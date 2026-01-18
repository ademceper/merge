using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Commands.CreateInventory;

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

