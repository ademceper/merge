using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using Merge.Domain.Enums;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.ML.Commands.PatchFraudDetectionRule;

/// <summary>
/// Handler for PatchFraudDetectionRuleCommand
/// HIGH-API-001: PATCH Support - Partial updates implementation
/// </summary>
public class PatchFraudDetectionRuleCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<PatchFraudDetectionRuleCommandHandler> logger) : IRequestHandler<PatchFraudDetectionRuleCommand, bool>
{
    public async Task<bool> Handle(PatchFraudDetectionRuleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Patching fraud detection rule. RuleId: {RuleId}", request.Id);

        var rule = await context.Set<FraudDetectionRule>()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (rule == null)
        {
            logger.LogWarning("Fraud detection rule not found. RuleId: {RuleId}", request.Id);
            throw new NotFoundException("Fraud detection rule", request.Id);
        }

        // Apply partial updates using domain methods
        if (!string.IsNullOrEmpty(request.PatchDto.Name))
            rule.UpdateName(request.PatchDto.Name);
        
        if (!string.IsNullOrEmpty(request.PatchDto.RuleType) && Enum.TryParse<FraudRuleType>(request.PatchDto.RuleType, true, out var ruleType))
            rule.UpdateRuleType(ruleType);
        
        if (request.PatchDto.Conditions != null)
            rule.UpdateConditions(JsonSerializer.Serialize(request.PatchDto.Conditions));
        
        if (request.PatchDto.RiskScore.HasValue)
            rule.UpdateRiskScore(request.PatchDto.RiskScore.Value);
        
        if (!string.IsNullOrEmpty(request.PatchDto.Action) && Enum.TryParse<FraudAction>(request.PatchDto.Action, true, out var action))
            rule.UpdateAction(action);
        
        if (request.PatchDto.Priority.HasValue)
            rule.UpdatePriority(request.PatchDto.Priority.Value);
        
        if (request.PatchDto.Description != null)
            rule.UpdateDescription(request.PatchDto.Description);

        if (request.PatchDto.IsActive.HasValue)
        {
            if (request.PatchDto.IsActive.Value && !rule.IsActive)
            {
                rule.Activate();
            }
            else if (!request.PatchDto.IsActive.Value && rule.IsActive)
            {
                rule.Deactivate();
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Fraud detection rule patched successfully. RuleId: {RuleId}", request.Id);

        return true;
    }
}
