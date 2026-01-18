using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.ML.Commands.CreateFraudDetectionRule;

public record CreateFraudDetectionRuleCommand(
    string Name,
    string RuleType,
    FraudRuleConditionsDto? Conditions,
    int RiskScore,
    string Action,
    bool IsActive,
    int Priority,
    string? Description) : IRequest<FraudDetectionRuleDto>;
