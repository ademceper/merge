using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Catalog.Commands.ReleaseStock;

public record ReleaseStockCommand(
    Guid ProductId,
    Guid WarehouseId,
    int Quantity,
    Guid? OrderId,
    Guid PerformedBy
) : IRequest<bool>;

