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

namespace Merge.Application.Governance.Commands.RevokeAcceptance;

public class RevokeAcceptanceCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ICacheService cache,
    ILogger<RevokeAcceptanceCommandHandler> logger) : IRequestHandler<RevokeAcceptanceCommand, bool>
{

    public async Task<bool> Handle(RevokeAcceptanceCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Revoking policy acceptance. UserId: {UserId}, PolicyId: {PolicyId}",
            request.UserId, request.PolicyId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var acceptance = await context.Set<PolicyAcceptance>()
                .FirstOrDefaultAsync(pa => pa.UserId == request.UserId && 
                                      pa.PolicyId == request.PolicyId && 
                                      pa.IsActive, cancellationToken);

            if (acceptance == null)
            {
                logger.LogWarning("Policy acceptance not found. UserId: {UserId}, PolicyId: {PolicyId}",
                    request.UserId, request.PolicyId);
                return false;
            }

            acceptance.Revoke();

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            await cache.RemoveAsync($"user_acceptances_{request.UserId}", cancellationToken);
            await cache.RemoveAsync($"pending_policies_{request.UserId}", cancellationToken);
            await cache.RemoveAsync($"policy_{request.PolicyId}", cancellationToken);

            logger.LogInformation("Policy acceptance revoked. AcceptanceId: {AcceptanceId}, UserId: {UserId}, PolicyId: {PolicyId}",
                acceptance.Id, request.UserId, request.PolicyId);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            logger.LogError(ex, "Concurrency conflict while revoking acceptance for policy {PolicyId}", request.PolicyId);
            throw new BusinessException("Policy kabul iptal çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error revoking acceptance for policy {PolicyId}", request.PolicyId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

