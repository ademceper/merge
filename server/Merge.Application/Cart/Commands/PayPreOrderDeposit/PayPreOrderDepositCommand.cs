using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.PayPreOrderDeposit;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record PayPreOrderDepositCommand(
    Guid UserId,
    Guid PreOrderId,
    decimal Amount) : IRequest<bool>;

