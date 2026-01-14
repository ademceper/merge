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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateFraudDetectionRuleCommandHandler : IRequestHandler<CreateFraudDetectionRuleCommand, FraudDetectionRuleDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateFraudDetectionRuleCommandHandler> _logger;

    public CreateFraudDetectionRuleCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateFraudDetectionRuleCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FraudDetectionRuleDto> Handle(CreateFraudDetectionRuleCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Creating fraud detection rule. Name: {Name}, RuleType: {RuleType}, RiskScore: {RiskScore}",
            request.Name, request.RuleType, request.RiskScore);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var ruleType = Enum.TryParse<FraudRuleType>(request.RuleType, true, out var rt) ? rt : FraudRuleType.Order;
        var action = Enum.TryParse<FraudAction>(request.Action, true, out var act) ? act : FraudAction.Flag;
        var conditions = request.Conditions != null ? JsonSerializer.Serialize(request.Conditions) : string.Empty;
        
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

        await _context.Set<FraudDetectionRule>().AddAsync(rule, cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage'lar oluşturulur
        // Background worker OutboxMessage'ları işleyip MediatR notification olarak dispatch eder
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ PERFORMANCE: Reload in one query (N+1 fix)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var createdRule = await _context.Set<FraudDetectionRule>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == rule.Id, cancellationToken);

        if (createdRule == null)
        {
            _logger.LogWarning("Fraud detection rule not found after creation. RuleId: {RuleId}", rule.Id);
            throw new NotFoundException("Fraud detection rule", rule.Id);
        }

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Fraud detection rule created. RuleId: {RuleId}, Name: {Name}",
            rule.Id, request.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<FraudDetectionRuleDto>(createdRule);
    }
}
