using MediatR;
using Merge.Application.DTOs.ML;

namespace Merge.Application.ML.Commands.PatchFraudDetectionRule;

/// <summary>
/// PATCH command for partial fraud detection rule updates
/// HIGH-API-001: PATCH Support - Partial updates without requiring all fields
/// </summary>
public record PatchFraudDetectionRuleCommand(
    Guid Id,
    PatchFraudDetectionRuleDto PatchDto
) : IRequest<bool>;
