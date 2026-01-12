using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.MoveToCart;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record MoveToCartCommand(
    Guid UserId,
    Guid ItemId
) : IRequest<bool>;

