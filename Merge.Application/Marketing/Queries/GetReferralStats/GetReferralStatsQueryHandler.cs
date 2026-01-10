using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Marketing.Queries.GetReferralStats;

public class GetReferralStatsQueryHandler : IRequestHandler<GetReferralStatsQuery, ReferralStatsDto>
{
    private readonly IDbContext _context;

    public GetReferralStatsQueryHandler(IDbContext context)
    {
        _context = context;
    }

    public async Task<ReferralStatsDto> Handle(GetReferralStatsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var totalReferrals = await _context.Set<Referral>()
            .AsNoTracking()
            .CountAsync(r => r.ReferrerId == request.UserId, cancellationToken);

        var completedReferrals = await _context.Set<Referral>()
            .AsNoTracking()
            .CountAsync(r => r.ReferrerId == request.UserId && (r.Status == ReferralStatus.Completed || r.Status == ReferralStatus.Rewarded), cancellationToken);

        var pendingReferrals = await _context.Set<Referral>()
            .AsNoTracking()
            .CountAsync(r => r.ReferrerId == request.UserId && r.Status == ReferralStatus.Pending, cancellationToken);

        var totalPointsAwarded = (int)await _context.Set<Referral>()
            .AsNoTracking()
            .Where(r => r.ReferrerId == request.UserId)
            .SumAsync(r => (long)r.PointsAwarded, cancellationToken);

        var conversionRate = totalReferrals > 0 
            ? (decimal)(totalReferrals - pendingReferrals) / totalReferrals * 100 
            : 0;

        return new ReferralStatsDto(
            totalReferrals,
            completedReferrals,
            pendingReferrals,
            totalPointsAwarded,
            conversionRate);
    }
}
