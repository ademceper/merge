using MediatR;

namespace Merge.Application.Catalog.Commands.DeleteInventory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteInventoryCommand(
    Guid Id,
    Guid PerformedBy
) : IRequest<bool>;

