using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Governance.Commands.ActivatePolicy;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ActivatePolicyCommandHandler : IRequestHandler<ActivatePolicyCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly ILogger<ActivatePolicyCommandHandler> _logger;
    private const string CACHE_KEY_ALL_POLICIES = "policies_all";
    private const string CACHE_KEY_ACTIVE_POLICIES = "policies_active";

    public ActivatePolicyCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        ILogger<ActivatePolicyCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> Handle(ActivatePolicyCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating policy. PolicyId: {PolicyId}", request.Id);

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

            // Deactivate other versions of the same type
            var existingPolicies = await _context.Set<Policy>()
                .Where(p => p.PolicyType == policy.PolicyType && 
                       p.Language == policy.Language && 
                       p.IsActive &&
                       p.Id != request.Id)
                .ToListAsync(cancellationToken);

            foreach (var existing in existingPolicies)
            {
                existing.Deactivate();
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            policy.Activate();

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_POLICIES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ACTIVE_POLICIES, cancellationToken);
            await _cache.RemoveAsync($"policy_{request.Id}", cancellationToken);
            await _cache.RemoveAsync($"policy_active_{policy.PolicyType}_{policy.Language}", cancellationToken);

            _logger.LogInformation("Policy activated. PolicyId: {PolicyId}, PolicyType: {PolicyType}, Version: {Version}",
                policy.Id, policy.PolicyType, policy.Version);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while activating policy {PolicyId}", request.Id);
            throw new BusinessException("Policy aktivasyon çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating policy {PolicyId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

