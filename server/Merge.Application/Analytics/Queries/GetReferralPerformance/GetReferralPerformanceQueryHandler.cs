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

public class GetReferralPerformanceQueryHandler(
    IDbContext context,
    ILogger<GetReferralPerformanceQueryHandler> logger) : IRequestHandler<GetReferralPerformanceQuery, ReferralPerformanceDto>
{

    public async Task<ReferralPerformanceDto> Handle(GetReferralPerformanceQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching referral performance. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        var referralsQuery = context.Set<Referral>()
            .AsNoTracking()
            .Where(r => r.CreatedAt >= request.StartDate && r.CreatedAt <= request.EndDate);

        var totalReferrals = await referralsQuery.CountAsync(cancellationToken);
        var successfulReferrals = await referralsQuery.CountAsync(r => r.CompletedAt != null, cancellationToken);
        var totalRewardsGiven = await referralsQuery.SumAsync(r => r.PointsAwarded, cancellationToken);

        return new ReferralPerformanceDto(
            totalReferrals,
            successfulReferrals,
            totalReferrals > 0 ? (decimal)successfulReferrals / totalReferrals * 100 : 0,
            totalRewardsGiven
        );
    }
}

