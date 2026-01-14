using MediatR;
using Merge.Application.DTOs.Payment;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.Payment.Commands.RefundPayment;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RefundPaymentCommand(
    Guid PaymentId,
    decimal? Amount = null
) : IRequest<PaymentDto>;
