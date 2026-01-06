using MediatR;
using Merge.Application.DTOs.Payment;

namespace Merge.Application.Payment.Commands.ProcessPayment;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ProcessPaymentCommand(
    Guid PaymentId,
    string TransactionId,
    string? PaymentReference = null,
    Dictionary<string, string>? Metadata = null
) : IRequest<PaymentDto>;
