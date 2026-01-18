using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Commands.UpdateInventory;

public record UpdateInventoryCommand(
    Guid Id,
    int MinimumStockLevel,
    int MaximumStockLevel,
    decimal UnitCost,
    string? Location,
    Guid PerformedBy
) : IRequest<InventoryDto>;

