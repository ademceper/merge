using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.ML.Commands.DeleteFraudDetectionRule;

public class DeleteFraudDetectionRuleCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteFraudDetectionRuleCommandHandler> logger) : IRequestHandler<DeleteFraudDetectionRuleCommand, bool>
{

    public async Task<bool> Handle(DeleteFraudDetectionRuleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting fraud detection rule. RuleId: {RuleId}", request.Id);

        var rule = await context.Set<FraudDetectionRule>()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (rule is null)
        {
            logger.LogWarning("Fraud detection rule not found. RuleId: {RuleId}", request.Id);
            return false;
        }

        rule.MarkAsDeleted();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Fraud detection rule deleted. RuleId: {RuleId}", request.Id);
        return true;
    }
}
