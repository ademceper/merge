using MediatR;

namespace Merge.Application.ML.Commands.DeleteFraudDetectionRule;

public record DeleteFraudDetectionRuleCommand(Guid Id) : IRequest<bool>;
