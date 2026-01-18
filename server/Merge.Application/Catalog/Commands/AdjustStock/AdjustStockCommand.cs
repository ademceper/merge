using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Commands.AdjustStock;

public record AdjustStockCommand(
    Guid InventoryId,
    int QuantityChange,
    string? Notes,
    Guid PerformedBy
) : IRequest<InventoryDto>;

