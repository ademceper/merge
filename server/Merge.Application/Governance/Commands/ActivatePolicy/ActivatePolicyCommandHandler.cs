using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Governance.Commands.ActivatePolicy;

public class ActivatePolicyCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<ActivatePolicyCommandHandler> logger) : IRequestHandler<ActivatePolicyCommand, bool>
{
    private const string CACHE_KEY_ALL_POLICIES = "policies_all";
    private const string CACHE_KEY_ACTIVE_POLICIES = "policies_active";

    public async Task<bool> Handle(ActivatePolicyCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Activating policy. PolicyId: {PolicyId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var policy = await context.Set<Policy>()
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (policy is null)
            {
                logger.LogWarning("Policy not found. PolicyId: {PolicyId}", request.Id);
                return false;
            }

            var existingPolicies = await context.Set<Policy>()
                .Where(p => p.PolicyType == policy.PolicyType && 
                       p.Language == policy.Language && 
                       p.IsActive &&
                       p.Id != request.Id)
                .ToListAsync(cancellationToken);

            foreach (var existing in existingPolicies)
            {
                existing.Deactivate();
            }

            policy.Activate();

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync(CACHE_KEY_ALL_POLICIES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_POLICIES, cancellationToken);
            await cache.RemoveAsync($"policy_{request.Id}", cancellationToken);
            await cache.RemoveAsync($"policy_active_{policy.PolicyType}_{policy.Language}", cancellationToken);

            logger.LogInformation("Policy activated. PolicyId: {PolicyId}, PolicyType: {PolicyType}, Version: {Version}",
                policy.Id, policy.PolicyType, policy.Version);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while activating policy {PolicyId}", request.Id);
            throw new BusinessException("Policy aktivasyon çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error activating policy {PolicyId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

