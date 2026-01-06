using MediatR;

namespace Merge.Application.Cart.Commands.PayPreOrderDeposit;

public record PayPreOrderDepositCommand(
    Guid UserId,
    Guid PreOrderId,
    decimal Amount) : IRequest<bool>;

