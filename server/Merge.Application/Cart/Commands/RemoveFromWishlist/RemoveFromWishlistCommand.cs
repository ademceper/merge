using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.RemoveFromWishlist;

public record RemoveFromWishlistCommand(
    Guid UserId,
    Guid ProductId
) : IRequest<bool>;

