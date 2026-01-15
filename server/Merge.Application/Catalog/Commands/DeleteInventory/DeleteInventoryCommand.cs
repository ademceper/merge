using MediatR;

namespace Merge.Application.Catalog.Commands.DeleteInventory;

public record DeleteInventoryCommand(
    Guid Id,
    Guid PerformedBy
) : IRequest<bool>;