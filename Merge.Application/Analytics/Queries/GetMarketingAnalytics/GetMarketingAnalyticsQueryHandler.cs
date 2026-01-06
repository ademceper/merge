using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Entities.Order;

namespace Merge.Application.Analytics.Queries.GetMarketingAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetMarketingAnalyticsQueryHandler : IRequestHandler<GetMarketingAnalyticsQuery, MarketingAnalyticsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetMarketingAnalyticsQueryHandler> _logger;

    public GetMarketingAnalyticsQueryHandler(
        IDbContext context,
        ILogger<GetMarketingAnalyticsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<MarketingAnalyticsDto> Handle(GetMarketingAnalyticsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching marketing analytics. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted, !cu.IsDeleted, !o.IsDeleted checks (Global Query Filter handles it)
        var coupons = await _context.Set<Coupon>()
            .AsNoTracking()
            .Where(c => c.IsActive)
            .CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Use CountAsync instead of ToListAsync().Count (database'de count)
        var couponUsageCount = await _context.Set<CouponUsage>()
            .AsNoTracking()
            .Where(cu => cu.CreatedAt >= request.StartDate && cu.CreatedAt <= request.EndDate)
            .CountAsync(cancellationToken);

        var totalDiscounts = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= request.StartDate && o.CreatedAt <= request.EndDate)
            .SumAsync(o => (o.CouponDiscount ?? 0) + (o.GiftCardDiscount ?? 0), cancellationToken);

        // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
        var topCoupons = await GetCouponPerformanceAsync(request.StartDate, request.EndDate, cancellationToken);
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU) - 1 eleman biliniyor
        var referralStats = new List<ReferralPerformanceDto>(1) { await GetReferralPerformanceAsync(request.StartDate, request.EndDate, cancellationToken) };
        
        return new MarketingAnalyticsDto(
            request.StartDate,
            request.EndDate,
            0, // TotalCampaigns - şimdilik 0
            coupons,
            couponUsageCount,  // ✅ Database'de count
            totalDiscounts,
            0, // EmailMarketingROI - şimdilik 0
            topCoupons,
            referralStats
        );
    }

    private async Task<List<CouponPerformanceDto>> GetCouponPerformanceAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return await _context.Set<CouponUsage>()
            .AsNoTracking()
            .Include(cu => cu.Coupon)
            .Include(cu => cu.Order)
            .Where(cu => cu.CreatedAt >= startDate && cu.CreatedAt <= endDate)
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

    private async Task<ReferralPerformanceDto> GetReferralPerformanceAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var referralsQuery = _context.Set<Referral>()
            .AsNoTracking()
            .Where(r => r.CreatedAt >= startDate && r.CreatedAt <= endDate);

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

