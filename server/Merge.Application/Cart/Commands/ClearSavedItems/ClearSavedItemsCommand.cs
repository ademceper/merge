using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.ClearSavedItems;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ClearSavedItemsCommand(Guid UserId) : IRequest<bool>;

