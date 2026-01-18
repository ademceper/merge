using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.IsInWishlist;

public record IsInWishlistQuery(
    Guid UserId,
    Guid ProductId
) : IRequest<bool>;

