using MediatR;

namespace Merge.Application.Payment.Commands.DeletePaymentMethod;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeletePaymentMethodCommand(Guid PaymentMethodId) : IRequest<bool>;
