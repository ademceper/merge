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

namespace Merge.Application.Governance.Commands.CreatePolicy;

public class CreatePolicyCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper,
    ILogger<CreatePolicyCommandHandler> logger) : IRequestHandler<CreatePolicyCommand, PolicyDto>
{
    private const string CACHE_KEY_ALL_POLICIES = "policies_all";
    private const string CACHE_KEY_ACTIVE_POLICIES = "policies_active";

    public async Task<PolicyDto> Handle(CreatePolicyCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating policy. PolicyType: {PolicyType}, Version: {Version}, CreatedByUserId: {CreatedByUserId}",
            request.PolicyType, request.Version, request.CreatedByUserId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var policy = Policy.Create(
                policyType: request.PolicyType,
                title: request.Title,
                content: request.Content,
                version: request.Version,
                createdByUserId: request.CreatedByUserId,
                isActive: request.IsActive,
                requiresAcceptance: request.RequiresAcceptance,
                effectiveDate: request.EffectiveDate ?? DateTime.UtcNow,
                expiryDate: request.ExpiryDate,
                changeLog: request.ChangeLog,
                language: request.Language);

            if (request.IsActive)
            {
                var existingPolicies = await context.Set<Policy>()
                    .Where(p => p.PolicyType == request.PolicyType && 
                           p.Language == request.Language && 
                           p.IsActive)
                    .ToListAsync(cancellationToken);

                foreach (var existing in existingPolicies)
                {
                    existing.Deactivate();
                }
            }

            await context.Set<Policy>().AddAsync(policy, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var reloadedPolicy = await context.Set<Policy>()
                .AsNoTracking()
                .Include(p => p.CreatedBy)
                .FirstOrDefaultAsync(p => p.Id == policy.Id, cancellationToken);

            if (reloadedPolicy == null)
            {
                logger.LogWarning("Policy {PolicyId} not found after creation", policy.Id);
                throw new NotFoundException("Policy", policy.Id);
            }

            await cache.RemoveAsync(CACHE_KEY_ALL_POLICIES, cancellationToken);
            await cache.RemoveAsync(CACHE_KEY_ACTIVE_POLICIES, cancellationToken);
            await cache.RemoveAsync($"policy_{policy.Id}", cancellationToken);
            await cache.RemoveAsync($"policy_active_{policy.PolicyType}_{policy.Language}", cancellationToken);

            logger.LogInformation("Policy created. PolicyId: {PolicyId}, PolicyType: {PolicyType}, Version: {Version}",
                policy.Id, policy.PolicyType, policy.Version);

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
            logger.LogError(ex, "Concurrency conflict while creating policy with PolicyType: {PolicyType}", request.PolicyType);
            throw new BusinessException("Policy oluşturma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating policy with PolicyType: {PolicyType}", request.PolicyType);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

