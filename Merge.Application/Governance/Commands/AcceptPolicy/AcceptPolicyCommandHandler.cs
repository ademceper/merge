using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Governance;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Governance.Commands.AcceptPolicy;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AcceptPolicyCommandHandler : IRequestHandler<AcceptPolicyCommand, PolicyAcceptanceDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IMapper _mapper;
    private readonly ILogger<AcceptPolicyCommandHandler> _logger;

    public AcceptPolicyCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IMapper mapper,
        ILogger<AcceptPolicyCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PolicyAcceptanceDto> Handle(AcceptPolicyCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Accepting policy. UserId: {UserId}, PolicyId: {PolicyId}",
            request.UserId, request.PolicyId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var policy = await _context.Set<Policy>()
                .FirstOrDefaultAsync(p => p.Id == request.PolicyId && p.IsActive, cancellationToken);

            if (policy == null)
            {
                _logger.LogWarning("Policy not found or not active. PolicyId: {PolicyId}", request.PolicyId);
                throw new NotFoundException("Policy", request.PolicyId);
            }

            // Check if user already accepted this version
            var existingAcceptance = await _context.Set<PolicyAcceptance>()
                .Include(pa => pa.Policy)
                .Include(pa => pa.User)
                .FirstOrDefaultAsync(pa => pa.UserId == request.UserId && 
                                      pa.PolicyId == request.PolicyId && 
                                      pa.AcceptedVersion == policy.Version &&
                                      pa.IsActive, cancellationToken);

            if (existingAcceptance != null && existingAcceptance.IsActive)
            {
                _logger.LogInformation("Policy already accepted. AcceptanceId: {AcceptanceId}, UserId: {UserId}, PolicyId: {PolicyId}",
                    existingAcceptance.Id, request.UserId, request.PolicyId);
                return _mapper.Map<PolicyAcceptanceDto>(existingAcceptance);
            }

            // Deactivate old acceptances for this policy
            var oldAcceptances = await _context.Set<PolicyAcceptance>()
                .Where(pa => pa.UserId == request.UserId && pa.PolicyId == request.PolicyId)
                .ToListAsync(cancellationToken);

            foreach (var old in oldAcceptances)
            {
                if (old.IsActive)
                {
                    old.Revoke();
                }
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var acceptance = PolicyAcceptance.Create(
                policyId: request.PolicyId,
                userId: request.UserId,
                acceptedVersion: policy.Version,
                ipAddress: request.IpAddress ?? string.Empty,
                userAgent: request.UserAgent ?? string.Empty);

            await _context.Set<PolicyAcceptance>().AddAsync(acceptance, cancellationToken);
            // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
            // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler aynı transaction içinde OutboxMessage'lar olarak kaydedilir
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with Include instead of LoadAsync (N+1 fix)
            var reloadedAcceptance = await _context.Set<PolicyAcceptance>()
                .AsNoTracking()
                .Include(pa => pa.Policy)
                .Include(pa => pa.User)
                .FirstOrDefaultAsync(pa => pa.Id == acceptance.Id, cancellationToken);

            if (reloadedAcceptance == null)
            {
                _logger.LogWarning("Policy acceptance {AcceptanceId} not found after creation", acceptance.Id);
                throw new NotFoundException("Policy acceptance", acceptance.Id);
            }

            // ✅ BOLUM 10.2: Cache invalidation
            await _cache.RemoveAsync($"user_acceptances_{request.UserId}", cancellationToken);
            await _cache.RemoveAsync($"pending_policies_{request.UserId}", cancellationToken);
            await _cache.RemoveAsync($"policy_{request.PolicyId}", cancellationToken);

            _logger.LogInformation("Policy accepted. AcceptanceId: {AcceptanceId}, UserId: {UserId}, PolicyId: {PolicyId}",
                acceptance.Id, request.UserId, request.PolicyId);

            return _mapper.Map<PolicyAcceptanceDto>(reloadedAcceptance);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while accepting policy {PolicyId}", request.PolicyId);
            throw new BusinessException("Policy kabul çakışması. Lütfen tekrar deneyin.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting policy {PolicyId}", request.PolicyId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

