using MediatR;

namespace Merge.Application.ML.Commands.DeleteFraudDetectionRule;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteFraudDetectionRuleCommand(Guid Id) : IRequest<bool>;
