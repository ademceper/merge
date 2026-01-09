using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Governance;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Governance.Commands.CreatePolicy;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreatePolicyCommandHandler : IRequestHandler<CreatePolicyCommand, PolicyDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePolicyCommandHandler> _logger;
    private const string CACHE_KEY_ALL_POLICIES = "policies_all";
    private const string CACHE_KEY_ACTIVE_POLICIES = "policies_active";

    public CreatePolicyCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        ILogger<CreatePolicyCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PolicyDto> Handle(CreatePolicyCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating policy. PolicyType: {PolicyType}, Version: {Version}, CreatedByUserId: {CreatedByUserId}",
            request.PolicyType, request.Version, request.CreatedByUserId);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
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

            // If activating a new version, deactivate old versions of the same type
            if (request.IsActive)
            {
                var existingPolicies = await _context.Set<Policy>()
                    .Where(p => p.PolicyType == request.PolicyType && 
                           p.Language == request.Language && 
                           p.IsActive)
                    .ToListAsync(cancellationToken);

                foreach (var existing in existingPolicies)
                {
                    existing.Deactivate();
                }
            }

            await _context.Set<Policy>().AddAsync(policy, cancellationToken);
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
                _logger.LogWarning("Policy {PolicyId} not found after creation", policy.Id);
                throw new NotFoundException("Policy", policy.Id);
            }

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync(CACHE_KEY_ALL_POLICIES, cancellationToken);
            await _cache.RemoveAsync(CACHE_KEY_ACTIVE_POLICIES, cancellationToken);
            await _cache.RemoveAsync($"policy_{policy.Id}", cancellationToken);
            await _cache.RemoveAsync($"policy_active_{policy.PolicyType}_{policy.Language}", cancellationToken);

            _logger.LogInformation("Policy created. PolicyId: {PolicyId}, PolicyType: {PolicyType}, Version: {Version}",
                policy.Id, policy.PolicyType, policy.Version);

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
            _logger.LogError(ex, "Concurrency conflict while creating policy with PolicyType: {PolicyType}", request.PolicyType);
            throw new BusinessException("Policy oluşturma çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error creating policy with PolicyType: {PolicyType}", request.PolicyType);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

