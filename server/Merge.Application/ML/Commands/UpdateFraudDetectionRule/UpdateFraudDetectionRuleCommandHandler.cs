using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.ML.Commands.UpdateFraudDetectionRule;

public class UpdateFraudDetectionRuleCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<UpdateFraudDetectionRuleCommandHandler> logger) : IRequestHandler<UpdateFraudDetectionRuleCommand, bool>
{

    public async Task<bool> Handle(UpdateFraudDetectionRuleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating fraud detection rule. RuleId: {RuleId}, Name: {Name}",
            request.Id, request.Name);

        var rule = await context.Set<FraudDetectionRule>()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (rule is null)
        {
            logger.LogWarning("Fraud detection rule not found. RuleId: {RuleId}", request.Id);
            return false;
        }

        if (!string.IsNullOrEmpty(request.Name))
            rule.UpdateName(request.Name);
        
        if (!string.IsNullOrEmpty(request.RuleType) && Enum.TryParse<FraudRuleType>(request.RuleType, true, out var ruleType))
            rule.UpdateRuleType(ruleType);
        
        if (request.Conditions is not null)
            rule.UpdateConditions(JsonSerializer.Serialize(request.Conditions));
        
        rule.UpdateRiskScore(request.RiskScore);
        
        if (!string.IsNullOrEmpty(request.Action) && Enum.TryParse<FraudAction>(request.Action, true, out var action))
            rule.UpdateAction(action);
        
        rule.UpdatePriority(request.Priority);
        
        rule.UpdateDescription(request.Description);

        if (request.IsActive && !rule.IsActive)
        {
            rule.Activate();
        }
        else if (!request.IsActive && rule.IsActive)
        {
            rule.Deactivate();
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Fraud detection rule updated. RuleId: {RuleId}", request.Id);
        return true;
    }
}
