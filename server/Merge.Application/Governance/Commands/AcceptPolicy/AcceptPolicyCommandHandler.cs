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
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Governance.Commands.AcceptPolicy;

public class AcceptPolicyCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    IMapper mapper,
    ILogger<AcceptPolicyCommandHandler> logger) : IRequestHandler<AcceptPolicyCommand, PolicyAcceptanceDto>
{

    public async Task<PolicyAcceptanceDto> Handle(AcceptPolicyCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Accepting policy. UserId: {UserId}, PolicyId: {PolicyId}",
            request.UserId, request.PolicyId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var policy = await context.Set<Policy>()
                .FirstOrDefaultAsync(p => p.Id == request.PolicyId && p.IsActive, cancellationToken);

            if (policy == null)
            {
                logger.LogWarning("Policy not found or not active. PolicyId: {PolicyId}", request.PolicyId);
                throw new NotFoundException("Policy", request.PolicyId);
            }

            var existingAcceptance = await context.Set<PolicyAcceptance>()
            .AsSplitQuery()
                .Include(pa => pa.Policy)
                .Include(pa => pa.User)
                .FirstOrDefaultAsync(pa => pa.UserId == request.UserId && 
                                      pa.PolicyId == request.PolicyId && 
                                      pa.AcceptedVersion == policy.Version &&
                                      pa.IsActive, cancellationToken);

            if (existingAcceptance != null && existingAcceptance.IsActive)
            {
                logger.LogInformation("Policy already accepted. AcceptanceId: {AcceptanceId}, UserId: {UserId}, PolicyId: {PolicyId}",
                    existingAcceptance.Id, request.UserId, request.PolicyId);
                return mapper.Map<PolicyAcceptanceDto>(existingAcceptance);
            }

            var oldAcceptances = await context.Set<PolicyAcceptance>()
                .Where(pa => pa.UserId == request.UserId && pa.PolicyId == request.PolicyId)
                .ToListAsync(cancellationToken);

            foreach (var old in oldAcceptances)
            {
                if (old.IsActive)
                {
                    old.Revoke();
                }
            }

            var acceptance = PolicyAcceptance.Create(
                policyId: request.PolicyId,
                userId: request.UserId,
                acceptedVersion: policy.Version,
                ipAddress: request.IpAddress ?? string.Empty,
                userAgent: request.UserAgent ?? string.Empty);

            await context.Set<PolicyAcceptance>().AddAsync(acceptance, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            var reloadedAcceptance = await context.Set<PolicyAcceptance>()
                .AsNoTracking()
            .AsSplitQuery()
                .Include(pa => pa.Policy)
                .Include(pa => pa.User)
                .FirstOrDefaultAsync(pa => pa.Id == acceptance.Id, cancellationToken);

            if (reloadedAcceptance == null)
            {
                logger.LogWarning("Policy acceptance {AcceptanceId} not found after creation", acceptance.Id);
                throw new NotFoundException("Policy acceptance", acceptance.Id);
            }

            await cache.RemoveAsync($"user_acceptances_{request.UserId}", cancellationToken);
            await cache.RemoveAsync($"pending_policies_{request.UserId}", cancellationToken);
            await cache.RemoveAsync($"policy_{request.PolicyId}", cancellationToken);

            logger.LogInformation("Policy accepted. AcceptanceId: {AcceptanceId}, UserId: {UserId}, PolicyId: {PolicyId}",
                acceptance.Id, request.UserId, request.PolicyId);

            return mapper.Map<PolicyAcceptanceDto>(reloadedAcceptance);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while accepting policy {PolicyId}", request.PolicyId);
            throw new BusinessException("Policy kabul çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error accepting policy {PolicyId}", request.PolicyId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

