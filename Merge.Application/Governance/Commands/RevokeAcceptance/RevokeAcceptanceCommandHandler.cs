using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Governance.Commands.RevokeAcceptance;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RevokeAcceptanceCommandHandler : IRequestHandler<RevokeAcceptanceCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<RevokeAcceptanceCommandHandler> _logger;

    public RevokeAcceptanceCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<RevokeAcceptanceCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(RevokeAcceptanceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Revoking policy acceptance. UserId: {UserId}, PolicyId: {PolicyId}",
            request.UserId, request.PolicyId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // ✅ BOLUM 3.2: IDOR Koruması - Kullanıcı sadece kendi acceptance'larını iptal edebilir
            var acceptance = await _context.Set<PolicyAcceptance>()
                .FirstOrDefaultAsync(pa => pa.UserId == request.UserId && 
                                      pa.PolicyId == request.PolicyId && 
                                      pa.IsActive, cancellationToken);

            if (acceptance == null)
            {
                _logger.LogWarning("Policy acceptance not found. UserId: {UserId}, PolicyId: {PolicyId}",
                    request.UserId, request.PolicyId);
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            acceptance.Revoke();

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"user_acceptances_{request.UserId}", cancellationToken);
            await _cache.RemoveAsync($"pending_policies_{request.UserId}", cancellationToken);
            await _cache.RemoveAsync($"policy_{request.PolicyId}", cancellationToken);

            _logger.LogInformation("Policy acceptance revoked. AcceptanceId: {AcceptanceId}, UserId: {UserId}, PolicyId: {PolicyId}",
                acceptance.Id, request.UserId, request.PolicyId);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while revoking acceptance for policy {PolicyId}", request.PolicyId);
            throw new BusinessException("Policy kabul iptal çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking acceptance for policy {PolicyId}", request.PolicyId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

