using MediatR;

namespace Merge.Application.Catalog.Commands.UpdateLastCountDate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateLastCountDateCommand(
    Guid InventoryId,
    Guid PerformedBy
) : IRequest<bool>;

