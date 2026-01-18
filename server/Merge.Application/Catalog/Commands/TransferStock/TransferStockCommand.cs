using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Commands.TransferStock;

public record TransferStockCommand(
    Guid ProductId,
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    int Quantity,
    string? Notes,
    Guid PerformedBy
) : IRequest<bool>;

