using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using FraudDetectionRule = Merge.Domain.Modules.Payment.FraudDetectionRule;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.ML.Queries.GetFraudDetectionRuleById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetFraudDetectionRuleByIdQueryHandler(IDbContext context, IMapper mapper, ILogger<GetFraudDetectionRuleByIdQueryHandler> logger) : IRequestHandler<GetFraudDetectionRuleByIdQuery, FraudDetectionRuleDto?>
{

    public async Task<FraudDetectionRuleDto?> Handle(GetFraudDetectionRuleByIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Getting fraud detection rule by ID. RuleId: {RuleId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var rule = await context.Set<FraudDetectionRule>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (rule == null)
        {
            logger.LogWarning("Fraud detection rule not found. RuleId: {RuleId}", request.Id);
            return null;
        }

        logger.LogInformation("Fraud detection rule retrieved. RuleId: {RuleId}, Name: {Name}",
            rule.Id, rule.Name);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<FraudDetectionRuleDto>(rule);
    }
}
