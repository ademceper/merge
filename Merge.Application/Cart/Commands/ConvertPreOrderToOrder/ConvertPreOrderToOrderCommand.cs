using MediatR;

namespace Merge.Application.Cart.Commands.ConvertPreOrderToOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ConvertPreOrderToOrderCommand(
    Guid PreOrderId) : IRequest<bool>;

