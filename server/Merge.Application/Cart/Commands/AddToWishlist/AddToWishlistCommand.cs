using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.AddToWishlist;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AddToWishlistCommand(
    Guid UserId,
    Guid ProductId
) : IRequest<bool>;

