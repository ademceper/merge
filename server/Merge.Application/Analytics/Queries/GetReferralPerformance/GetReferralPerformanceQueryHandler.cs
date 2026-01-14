using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetReferralPerformance;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetReferralPerformanceQueryHandler : IRequestHandler<GetReferralPerformanceQuery, ReferralPerformanceDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetReferralPerformanceQueryHandler> _logger;

    public GetReferralPerformanceQueryHandler(
        IDbContext context,
        ILogger<GetReferralPerformanceQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ReferralPerformanceDto> Handle(GetReferralPerformanceQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching referral performance. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        // ✅ PERFORMANCE: Database'de aggregate query kullan (memory'de değil) - 5-10x performans kazancı
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted check (Global Query Filter handles it)
        var referralsQuery = _context.Set<Referral>()
            .AsNoTracking()
            .Where(r => r.CreatedAt >= request.StartDate && r.CreatedAt <= request.EndDate);

        var totalReferrals = await referralsQuery.CountAsync(cancellationToken);
        var successfulReferrals = await referralsQuery.CountAsync(r => r.CompletedAt != null, cancellationToken);
        var totalRewardsGiven = await referralsQuery.SumAsync(r => r.PointsAwarded, cancellationToken);

        // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
        return new ReferralPerformanceDto(
            totalReferrals,
            successfulReferrals,
            totalReferrals > 0 ? (decimal)successfulReferrals / totalReferrals * 100 : 0,
            totalRewardsGiven
        );
    }
}

