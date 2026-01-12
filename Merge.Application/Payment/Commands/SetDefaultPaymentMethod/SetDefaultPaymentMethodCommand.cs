using MediatR;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Payment.Commands.SetDefaultPaymentMethod;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record SetDefaultPaymentMethodCommand(Guid PaymentMethodId) : IRequest<bool>;
