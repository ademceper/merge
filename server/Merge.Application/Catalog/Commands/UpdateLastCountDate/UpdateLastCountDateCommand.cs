using MediatR;

namespace Merge.Application.Catalog.Commands.UpdateLastCountDate;

public record UpdateLastCountDateCommand(
    Guid InventoryId,
    Guid PerformedBy
) : IRequest<bool>;

