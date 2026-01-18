using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.UpdateCartItem;

public record UpdateCartItemCommand(
    Guid CartItemId,
    int Quantity
) : IRequest<bool>;

