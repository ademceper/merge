using MediatR;

namespace Merge.Application.Cart.Commands.CancelPreOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CancelPreOrderCommand(
    Guid PreOrderId,
    Guid UserId) : IRequest<bool>;

