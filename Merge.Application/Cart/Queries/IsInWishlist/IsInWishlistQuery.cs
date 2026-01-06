using MediatR;

namespace Merge.Application.Cart.Queries.IsInWishlist;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record IsInWishlistQuery(
    Guid UserId,
    Guid ProductId
) : IRequest<bool>;

