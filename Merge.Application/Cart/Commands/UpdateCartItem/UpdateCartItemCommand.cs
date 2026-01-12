using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.UpdateCartItem;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateCartItemCommand(
    Guid CartItemId,
    int Quantity
) : IRequest<bool>;

