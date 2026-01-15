using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetCouponPerformance;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetCouponPerformanceQueryHandler(
    IDbContext context,
    ILogger<GetCouponPerformanceQueryHandler> logger) : IRequestHandler<GetCouponPerformanceQuery, List<CouponPerformanceDto>>
{

    public async Task<List<CouponPerformanceDto>> Handle(GetCouponPerformanceQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching coupon performance. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de değil) - 10x+ performans kazancı
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !cu.IsDeleted check (Global Query Filter handles it)
        return await context.Set<CouponUsage>()
            .AsNoTracking()
            .Include(cu => cu.Coupon)
            .Include(cu => cu.Order)
            .Where(cu => cu.CreatedAt >= request.StartDate && cu.CreatedAt <= request.EndDate)
            .GroupBy(cu => new { cu.CouponId, cu.Coupon.Code })
            // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
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

