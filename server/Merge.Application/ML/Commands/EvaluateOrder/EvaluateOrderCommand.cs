using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.ML.Commands.EvaluateOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record EvaluateOrderCommand(Guid OrderId) : IRequest<FraudAlertDto>;
