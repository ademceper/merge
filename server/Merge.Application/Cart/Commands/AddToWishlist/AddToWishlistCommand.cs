using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.AddToWishlist;

public record AddToWishlistCommand(
    Guid UserId,
    Guid ProductId
) : IRequest<bool>;

