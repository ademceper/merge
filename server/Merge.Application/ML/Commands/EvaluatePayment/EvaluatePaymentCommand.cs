using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.ML.Commands.EvaluatePayment;

public record EvaluatePaymentCommand(Guid PaymentId) : IRequest<FraudAlertDto>;
