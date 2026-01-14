using MediatR;

namespace Merge.Application.Subscription.Commands.ProcessPayment;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ProcessPaymentCommand(
    Guid PaymentId,
    string TransactionId) : IRequest<bool>;
