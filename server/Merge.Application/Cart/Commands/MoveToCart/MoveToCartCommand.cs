using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.MoveToCart;

public record MoveToCartCommand(
    Guid UserId,
    Guid ItemId
) : IRequest<bool>;

