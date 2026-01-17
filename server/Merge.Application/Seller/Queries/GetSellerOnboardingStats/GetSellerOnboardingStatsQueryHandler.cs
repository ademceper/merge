using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetSellerOnboardingStats;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSellerOnboardingStatsQueryHandler(IDbContext context, ILogger<GetSellerOnboardingStatsQueryHandler> logger) : IRequestHandler<GetSellerOnboardingStatsQuery, SellerOnboardingStatsDto>
{

    public async Task<SellerOnboardingStatsDto> Handle(GetSellerOnboardingStatsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Getting seller onboarding stats");

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var stats = await context.Set<SellerApplication>()
            .AsNoTracking()
            .GroupBy(a => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Pending = g.Count(a => a.Status == SellerApplicationStatus.Pending),
                Approved = g.Count(a => a.Status == SellerApplicationStatus.Approved),
                Rejected = g.Count(a => a.Status == SellerApplicationStatus.Rejected)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var thisMonth = DateTime.UtcNow.AddMonths(-1);
        var approvedThisMonth = await context.Set<SellerApplication>()
            .AsNoTracking()
            .CountAsync(a => a.Status == SellerApplicationStatus.Approved &&
                           a.ApprovedAt >= thisMonth, cancellationToken);

        var total = stats?.Total ?? 0;
        var approved = stats?.Approved ?? 0;

        return new SellerOnboardingStatsDto
        {
            TotalApplications = total,
            PendingApplications = stats?.Pending ?? 0,
            ApprovedApplications = approved,
            RejectedApplications = stats?.Rejected ?? 0,
            ApprovedThisMonth = approvedThisMonth,
            ApprovalRate = total > 0 ? (approved * 100.0m / total) : 0
        };
    }
}
