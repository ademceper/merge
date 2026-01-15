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

namespace Merge.Application.Governance.Commands.DeletePolicy;

public class DeletePolicyCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<DeletePolicyCommandHandler> logger) : IRequestHandler<DeletePolicyCommand, bool>
{
    private const string CACHE_KEY_ALL_POLICIES = "policies_all";
    private const string CACHE_KEY_ACTIVE_POLICIES = "policies_active";

    public async Task<bool> Handle(DeletePolicyCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting policy. PolicyId: {PolicyId}", request.Id);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var policy = await context.Set<Policy>()
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (policy == null)
            {
                logger.LogWarning("Policy not found for deletion. PolicyId: {PolicyId}", request.Id);
                return false;
            }

            policy.MarkAsDeleted();

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync(CACHE_KEY_ALL_POLICIES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_POLICIES, cancellationToken);
            await cache.RemoveAsync($"policy_{request.Id}", cancellationToken);
            await cache.RemoveAsync($"policy_active_{policy.PolicyType}_{policy.Language}", cancellationToken);

            logger.LogInformation("Policy deleted (soft delete). PolicyId: {PolicyId}", request.Id);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while deleting policy {PolicyId}", request.Id);
            throw new BusinessException("Policy silme çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting policy {PolicyId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

