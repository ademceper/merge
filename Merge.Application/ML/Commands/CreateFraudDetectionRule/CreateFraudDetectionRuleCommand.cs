using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.ML.Commands.CreateFraudDetectionRule;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateFraudDetectionRuleCommand(
    string Name,
    string RuleType,
    FraudRuleConditionsDto? Conditions,
    int RiskScore,
    string Action,
    bool IsActive,
    int Priority,
    string? Description) : IRequest<FraudDetectionRuleDto>;
