using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.CancelPreOrder;

public record CancelPreOrderCommand(
    Guid PreOrderId,
    Guid UserId) : IRequest<bool>;

