using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.ML.Commands.UpdateFraudDetectionRule;

public record UpdateFraudDetectionRuleCommand(
    Guid Id,
    string Name,
    string RuleType,
    FraudRuleConditionsDto? Conditions,
    int RiskScore,
    string Action,
    bool IsActive,
    int Priority,
    string? Description) : IRequest<bool>;
