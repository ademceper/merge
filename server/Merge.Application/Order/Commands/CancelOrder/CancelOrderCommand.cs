using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.CancelOrder;

public record CancelOrderCommand(
    Guid OrderId
) : IRequest<bool>;
