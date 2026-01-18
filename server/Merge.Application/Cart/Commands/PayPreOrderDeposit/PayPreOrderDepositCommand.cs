using MediatR;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.PayPreOrderDeposit;

public record PayPreOrderDepositCommand(
    Guid UserId,
    Guid PreOrderId,
    decimal Amount) : IRequest<bool>;

