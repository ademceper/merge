using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.RemoveCartItem;

public record RemoveCartItemCommand(Guid CartItemId) : IRequest<bool>;

