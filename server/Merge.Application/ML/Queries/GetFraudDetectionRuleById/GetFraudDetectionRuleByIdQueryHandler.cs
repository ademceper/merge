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

public class GetFraudDetectionRuleByIdQueryHandler(IDbContext context, IMapper mapper, ILogger<GetFraudDetectionRuleByIdQueryHandler> logger) : IRequestHandler<GetFraudDetectionRuleByIdQuery, FraudDetectionRuleDto?>
{

    public async Task<FraudDetectionRuleDto?> Handle(GetFraudDetectionRuleByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting fraud detection rule by ID. RuleId: {RuleId}", request.Id);

        var rule = await context.Set<FraudDetectionRule>()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (rule is null)
        {
            logger.LogWarning("Fraud detection rule not found. RuleId: {RuleId}", request.Id);
            return null;
        }

        logger.LogInformation("Fraud detection rule retrieved. RuleId: {RuleId}, Name: {Name}",
            rule.Id, rule.Name);

        return mapper.Map<FraudDetectionRuleDto>(rule);
    }
}
