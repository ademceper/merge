using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Order.Commands.CancelOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CancelOrderCommand(
    Guid OrderId
) : IRequest<bool>;
