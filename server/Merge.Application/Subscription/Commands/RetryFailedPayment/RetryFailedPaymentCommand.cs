using MediatR;

namespace Merge.Application.Subscription.Commands.RetryFailedPayment;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RetryFailedPaymentCommand(Guid PaymentId) : IRequest<bool>;
