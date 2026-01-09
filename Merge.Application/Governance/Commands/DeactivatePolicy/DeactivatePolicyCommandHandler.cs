using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Governance.Commands.DeactivatePolicy;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeactivatePolicyCommandHandler : IRequestHandler<DeactivatePolicyCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<DeactivatePolicyCommandHandler> _logger;
    private const string CACHE_KEY_ALL_POLICIES = "policies_all";
    private const string CACHE_KEY_ACTIVE_POLICIES = "policies_active";

    public DeactivatePolicyCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<DeactivatePolicyCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(DeactivatePolicyCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deactivating policy. PolicyId: {PolicyId}", request.Id);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var policy = await _context.Set<Policy>()
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (policy == null)
            {
                _logger.LogWarning("Policy not found. PolicyId: {PolicyId}", request.Id);
                return false;
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            policy.Deactivate();

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_POLICIES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ACTIVE_POLICIES, cancellationToken);
            await _cache.RemoveAsync($"policy_{request.Id}", cancellationToken);
            await _cache.RemoveAsync($"policy_active_{policy.PolicyType}_{policy.Language}", cancellationToken);

            _logger.LogInformation("Policy deactivated. PolicyId: {PolicyId}, PolicyType: {PolicyType}, Version: {Version}",
                policy.Id, policy.PolicyType, policy.Version);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while deactivating policy {PolicyId}", request.Id);
            throw new BusinessException("Policy deaktivasyon çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating policy {PolicyId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

