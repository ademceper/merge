using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetReferralStats;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetReferralStatsQueryHandler(IDbContext context) : IRequestHandler<GetReferralStatsQuery, ReferralStatsDto>
{
    public async Task<ReferralStatsDto> Handle(GetReferralStatsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var totalReferrals = await context.Set<Referral>()
            .AsNoTracking()
            .CountAsync(r => r.ReferrerId == request.UserId, cancellationToken);

        var completedReferrals = await context.Set<Referral>()
            .AsNoTracking()
            .CountAsync(r => r.ReferrerId == request.UserId && (r.Status == ReferralStatus.Completed || r.Status == ReferralStatus.Rewarded), cancellationToken);

        var pendingReferrals = await context.Set<Referral>()
            .AsNoTracking()
            .CountAsync(r => r.ReferrerId == request.UserId && r.Status == ReferralStatus.Pending, cancellationToken);

        var totalPointsAwarded = (int)await context.Set<Referral>()
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
