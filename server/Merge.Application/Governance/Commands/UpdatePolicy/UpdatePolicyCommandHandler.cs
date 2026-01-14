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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UpdatePolicyCommandHandler : IRequestHandler<UpdatePolicyCommand, PolicyDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdatePolicyCommandHandler> _logger;
    private const string CACHE_KEY_ALL_POLICIES = "policies_all";
    private const string CACHE_KEY_ACTIVE_POLICIES = "policies_active";

    public UpdatePolicyCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        ILogger<UpdatePolicyCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PolicyDto> Handle(UpdatePolicyCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating policy. PolicyId: {PolicyId}, UpdatedByUserId: {UpdatedByUserId}",
            request.Id, request.UpdatedByUserId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var policy = await _context.Set<Policy>()
                .Include(p => p.CreatedBy)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (policy == null)
            {
                _logger.LogWarning("Policy not found. PolicyId: {PolicyId}", request.Id);
                throw new NotFoundException("Policy", request.Id);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
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

            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            var reloadedPolicy = await _context.Set<Policy>()
                .AsNoTracking()
                .Include(p => p.CreatedBy)
                .FirstOrDefaultAsync(p => p.Id == policy.Id, cancellationToken);

            if (reloadedPolicy == null)
            {
                _logger.LogWarning("Policy {PolicyId} not found after update", policy.Id);
                throw new NotFoundException("Policy", policy.Id);
            }

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_POLICIES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ACTIVE_POLICIES, cancellationToken);
            await _cache.RemoveAsync($"policy_{policy.Id}", cancellationToken);
            await _cache.RemoveAsync($"policy_active_{policy.PolicyType}_{policy.Language}", cancellationToken);

            _logger.LogInformation("Policy updated. PolicyId: {PolicyId}, Version: {Version}",
                policy.Id, policy.Version);

            var policyDto = _mapper.Map<PolicyDto>(reloadedPolicy);
            
            // ✅ PERFORMANCE: AcceptanceCount database'de hesapla
            var acceptanceCount = await _context.Set<PolicyAcceptance>()
                .AsNoTracking()
                .CountAsync(pa => pa.PolicyId == policy.Id && pa.IsActive, cancellationToken);
            
            // ✅ BOLUM 7.1.5: Records - with expression kullanımı
            policyDto = policyDto with { AcceptanceCount = acceptanceCount };

            return policyDto;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while updating policy {PolicyId}", request.Id);
            throw new BusinessException("Policy güncelleme çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating policy {PolicyId}", request.Id);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

