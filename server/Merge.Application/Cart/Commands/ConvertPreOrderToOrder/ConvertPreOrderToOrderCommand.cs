using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.ConvertPreOrderToOrder;

public record ConvertPreOrderToOrderCommand(
    Guid PreOrderId) : IRequest<bool>;

