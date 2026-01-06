using MediatR;

namespace Merge.Application.Cart.Commands.RemoveCartItem;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RemoveCartItemCommand(Guid CartItemId) : IRequest<bool>;

