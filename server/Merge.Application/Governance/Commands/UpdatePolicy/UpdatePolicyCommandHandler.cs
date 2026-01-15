using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Governance;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Content;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Governance.Commands.UpdatePolicy;

public class UpdatePolicyCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper,
    ILogger<UpdatePolicyCommandHandler> logger) : IRequestHandler<UpdatePolicyCommand, PolicyDto>
{
    private const string CACHE_KEY_ALL_POLICIES = "policies_all";
    private const string CACHE_KEY_ACTIVE_POLICIES = "policies_active";

    public async Task<PolicyDto> Handle(UpdatePolicyCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating policy. PolicyId: {PolicyId}, UpdatedByUserId: {UpdatedByUserId}",
            request.Id, request.UpdatedByUserId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var policy = await context.Set<Policy>()
                .Include(p => p.CreatedBy)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (policy == null)
            {
                logger.LogWarning("Policy not found. PolicyId: {PolicyId}", request.Id);
                throw new NotFoundException("Policy", request.Id);
            }

            if (!string.IsNullOrEmpty(request.Title))
                policy.UpdateTitle(request.Title);
            if (!string.IsNullOrEmpty(request.Content))
                policy.UpdateContent(request.Content);
            if (!string.IsNullOrEmpty(request.Version))
                policy.UpdateVersion(request.Version);
            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                    policy.Activate();
                else
                    policy.Deactivate();
            }
            if (request.RequiresAcceptance.HasValue)
                policy.UpdateRequiresAcceptance(request.RequiresAcceptance.Value);
            if (request.EffectiveDate.HasValue)
                policy.UpdateEffectiveDate(request.EffectiveDate.Value);
            if (request.ExpiryDate.HasValue)
                policy.UpdateExpiryDate(request.ExpiryDate.Value);
            if (request.ChangeLog != null)
                policy.UpdateChangeLog(request.ChangeLog);

            policy.UpdateCreatedByUserId(request.UpdatedByUserId);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var reloadedPolicy = await context.Set<Policy>()
                .AsNoTracking()
                .Include(p => p.CreatedBy)
                .FirstOrDefaultAsync(p => p.Id == policy.Id, cancellationToken);

            if (reloadedPolicy == null)
            {
                logger.LogWarning("Policy {PolicyId} not found after update", policy.Id);
                throw new NotFoundException("Policy", policy.Id);
            }

            await cache.RemoveAsync(CACHE_KEY_ALL_POLICIES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_POLICIES, cancellationToken);
            await cache.RemoveAsync($"policy_{policy.Id}", cancellationToken);
            await cache.RemoveAsync($"policy_active_{policy.PolicyType}_{policy.Language}", cancellationToken);

            logger.LogInformation("Policy updated. PolicyId: {PolicyId}, Version: {Version}",
                policy.Id, policy.Version);

            var policyDto = mapper.Map<PolicyDto>(reloadedPolicy);
            
            var acceptanceCount = await context.Set<PolicyAcceptance>()
                .AsNoTracking()
                .CountAsync(pa => pa.PolicyId == policy.Id && pa.IsActive, cancellationToken);
            
            policyDto = policyDto with { AcceptanceCount = acceptanceCount };

            return policyDto;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while updating policy {PolicyId}", request.Id);
            throw new BusinessException("Policy güncelleme çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating policy {PolicyId}", request.Id);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

