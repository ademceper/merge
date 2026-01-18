using MediatR;

namespace Merge.Application.Subscription.Commands.ProcessPayment;

public record ProcessPaymentCommand(
    Guid PaymentId,
    string TransactionId) : IRequest<bool>;
