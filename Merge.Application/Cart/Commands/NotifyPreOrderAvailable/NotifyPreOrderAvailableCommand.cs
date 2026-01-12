using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.NotifyPreOrderAvailable;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record NotifyPreOrderAvailableCommand(
    Guid PreOrderId) : IRequest;

