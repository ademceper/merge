using MediatR;

namespace Merge.Application.Cart.Commands.RemoveFromWishlist;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RemoveFromWishlistCommand(
    Guid UserId,
    Guid ProductId
) : IRequest<bool>;

