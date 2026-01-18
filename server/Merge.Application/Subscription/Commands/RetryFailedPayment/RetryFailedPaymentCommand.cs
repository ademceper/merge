using MediatR;

namespace Merge.Application.Subscription.Commands.RetryFailedPayment;

public record RetryFailedPaymentCommand(Guid PaymentId) : IRequest<bool>;
