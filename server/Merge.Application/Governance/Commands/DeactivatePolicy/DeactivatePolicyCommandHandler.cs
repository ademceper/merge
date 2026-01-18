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

namespace Merge.Application.Governance.Commands.DeactivatePolicy;

public class DeactivatePolicyCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<DeactivatePolicyCommandHandler> logger) : IRequestHandler<DeactivatePolicyCommand, bool>
{
    private const string CACHE_KEY_ALL_POLICIES = "policies_all";
    private const string CACHE_KEY_ACTIVE_POLICIES = "policies_active";

    public async Task<bool> Handle(DeactivatePolicyCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deactivating policy. PolicyId: {PolicyId}", request.Id);

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

            policy.Deactivate();

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync(CACHE_KEY_ALL_POLICIES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_POLICIES, cancellationToken);
            await cache.RemoveAsync($"policy_{request.Id}", cancellationToken);
            await cache.RemoveAsync($"policy_active_{policy.PolicyType}_{policy.Language}", cancellationToken);

            logger.LogInformation("Policy deactivated. PolicyId: {PolicyId}, PolicyType: {PolicyType}, Version: {Version}",
                policy.Id, policy.PolicyType, policy.Version);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while deactivating policy {PolicyId}", request.Id);
            throw new BusinessException("Policy deaktivasyon çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deactivating policy {PolicyId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

