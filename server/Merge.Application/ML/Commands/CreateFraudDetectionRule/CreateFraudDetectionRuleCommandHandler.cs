using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.ML.Commands.CreateFraudDetectionRule;

public class CreateFraudDetectionRuleCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateFraudDetectionRuleCommandHandler> logger) : IRequestHandler<CreateFraudDetectionRuleCommand, FraudDetectionRuleDto>
{

    public async Task<FraudDetectionRuleDto> Handle(CreateFraudDetectionRuleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating fraud detection rule. Name: {Name}, RuleType: {RuleType}, RiskScore: {RiskScore}",
            request.Name, request.RuleType, request.RiskScore);

        var ruleType = Enum.TryParse<FraudRuleType>(request.RuleType, true, out var rt) ? rt : FraudRuleType.Order;
        var action = Enum.TryParse<FraudAction>(request.Action, true, out var act) ? act : FraudAction.Flag;
        var conditions = request.Conditions is not null ? JsonSerializer.Serialize(request.Conditions) : string.Empty;
        
        var rule = FraudDetectionRule.Create(
            name: request.Name,
            ruleType: ruleType,
            conditions: conditions,
            riskScore: request.RiskScore,
            action: action,
            priority: request.Priority,
            description: request.Description);
        
        if (!request.IsActive)
        {
            rule.Deactivate();
        }

        await context.Set<FraudDetectionRule>().AddAsync(rule, cancellationToken);
        
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var createdRule = await context.Set<FraudDetectionRule>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == rule.Id, cancellationToken);

        if (createdRule is null)
        {
            logger.LogWarning("Fraud detection rule not found after creation. RuleId: {RuleId}", rule.Id);
            throw new NotFoundException("Fraud detection rule", rule.Id);
        }

        logger.LogInformation("Fraud detection rule created. RuleId: {RuleId}, Name: {Name}",
            rule.Id, request.Name);

        return mapper.Map<FraudDetectionRuleDto>(createdRule);
    }
}
