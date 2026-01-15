using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Analytics.Queries.GetCouponPerformance;

public class GetCouponPerformanceQueryHandler(
    IDbContext context,
    ILogger<GetCouponPerformanceQueryHandler> logger) : IRequestHandler<GetCouponPerformanceQuery, List<CouponPerformanceDto>>
{

    public async Task<List<CouponPerformanceDto>> Handle(GetCouponPerformanceQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching coupon performance. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        return await context.Set<CouponUsage>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(cu => cu.Coupon)
            .Include(cu => cu.Order)
            .Where(cu => cu.CreatedAt >= request.StartDate && cu.CreatedAt <= request.EndDate)
            .GroupBy(cu => new { cu.CouponId, cu.Coupon.Code })
            .Select(g => new CouponPerformanceDto(
                g.Key.CouponId,
                g.Key.Code,
                g.Count(),
                g.Sum(cu => (cu.Order != null ? (cu.Order.CouponDiscount ?? 0) + (cu.Order.GiftCardDiscount ?? 0) : 0)),
                g.Sum(cu => cu.Order != null ? cu.Order.TotalAmount : 0)
            ))
            .OrderByDescending(c => c.UsageCount)
            .ToListAsync(cancellationToken);
    }
}

