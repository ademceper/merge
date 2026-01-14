using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.RemoveSavedItem;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RemoveSavedItemCommand(
    Guid UserId,
    Guid ItemId
) : IRequest<bool>;

