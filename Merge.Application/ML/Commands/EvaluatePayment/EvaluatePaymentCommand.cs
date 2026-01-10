using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.ML.Commands.EvaluatePayment;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record EvaluatePaymentCommand(Guid PaymentId) : IRequest<FraudAlertDto>;
