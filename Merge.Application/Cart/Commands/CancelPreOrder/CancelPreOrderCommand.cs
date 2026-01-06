using MediatR;

namespace Merge.Application.Cart.Commands.CancelPreOrder;

public record CancelPreOrderCommand(
    Guid PreOrderId,
    Guid UserId) : IRequest<bool>;

