using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;

namespace Merge.Application.ML.Commands.UpdateFraudDetectionRule;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateFraudDetectionRuleCommandHandler : IRequestHandler<UpdateFraudDetectionRuleCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateFraudDetectionRuleCommandHandler> _logger;

    public UpdateFraudDetectionRuleCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdateFraudDetectionRuleCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateFraudDetectionRuleCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Updating fraud detection rule. RuleId: {RuleId}, Name: {Name}",
            request.Id, request.Name);

        // ✅ PERFORMANCE: Removed manual !r.IsDeleted (Global Query Filter)
        var rule = await _context.Set<FraudDetectionRule>()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (rule == null)
        {
            _logger.LogWarning("Fraud detection rule not found. RuleId: {RuleId}", request.Id);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        if (!string.IsNullOrEmpty(request.Name))
            rule.UpdateName(request.Name);
        
        // ✅ BOLUM 1.2: Enum kullanımı (string RuleType YASAK)
        if (!string.IsNullOrEmpty(request.RuleType) && Enum.TryParse<FraudRuleType>(request.RuleType, true, out var ruleType))
            rule.UpdateRuleType(ruleType);
        
        if (request.Conditions != null)
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

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Fraud detection rule updated. RuleId: {RuleId}", request.Id);
        return true;
    }
}
