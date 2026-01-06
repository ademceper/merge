using MediatR;
using Merge.Application.DTOs.Payment;

namespace Merge.Application.Payment.Commands.CreatePayment;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreatePaymentCommand(
    Guid OrderId,
    string PaymentMethod,
    string PaymentProvider,
    decimal Amount
) : IRequest<PaymentDto>;
