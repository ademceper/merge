using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Subscription;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using PaymentStatus = Merge.Domain.Enums.PaymentStatus;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Subscription.Queries.GetSubscriptionAnalytics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSubscriptionAnalyticsQueryHandler : IRequestHandler<GetSubscriptionAnalyticsQuery, SubscriptionAnalyticsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetSubscriptionAnalyticsQueryHandler> _logger;

    public GetSubscriptionAnalyticsQueryHandler(
        IDbContext context,
        ILogger<GetSubscriptionAnalyticsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SubscriptionAnalyticsDto> Handle(GetSubscriptionAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var start = request.StartDate ?? DateTime.UtcNow.AddMonths(-12);
        var end = request.EndDate ?? DateTime.UtcNow;

        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        var query = _context.Set<UserSubscription>()
            .AsNoTracking()
            .Include(us => us.SubscriptionPlan)
            .Where(us => us.CreatedAt >= start && us.CreatedAt <= end);

        var totalSubscriptions = await query.CountAsync(cancellationToken);
        var activeSubscriptionsCount = await query.CountAsync(us => us.Status == SubscriptionStatus.Active && us.EndDate > DateTime.UtcNow, cancellationToken);
        var trialSubscriptionsCount = await query.CountAsync(us => us.Status == SubscriptionStatus.Trial, cancellationToken);
        var cancelledSubscriptionsCount = await query.CountAsync(us => us.Status == SubscriptionStatus.Cancelled, cancellationToken);

        var mrr = await query
            .Where(us => us.Status == SubscriptionStatus.Active && us.EndDate > DateTime.UtcNow)
            .SumAsync(us => (decimal?)us.CurrentPrice, cancellationToken) ?? 0;
        var arr = mrr * 12;

        var churnRate = totalSubscriptions > 0 
            ? (decimal)cancelledSubscriptionsCount / totalSubscriptions * 100 
            : 0;

        var arpu = activeSubscriptionsCount > 0
            ? await query
                .Where(us => us.Status == SubscriptionStatus.Active && us.EndDate > DateTime.UtcNow)
                .AverageAsync(us => (decimal?)us.CurrentPrice, cancellationToken) ?? 0
            : 0;

        // ✅ PERFORMANCE: Database'de grouping yap
        var subscriptionsByPlan = await query
            .GroupBy(us => us.SubscriptionPlan != null ? us.SubscriptionPlan.Name : "Unknown")
            .Select(g => new { PlanName = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PlanName, x => x.Count, cancellationToken);

        var revenueByPlan = await query
            .Where(us => us.Status == SubscriptionStatus.Active)
            .GroupBy(us => us.SubscriptionPlan != null ? us.SubscriptionPlan.Name : "Unknown")
            .Select(g => new { PlanName = g.Key, Revenue = g.Sum(us => us.CurrentPrice) })
            .ToDictionaryAsync(x => x.PlanName, x => x.Revenue, cancellationToken);

        // Get trends - Direct query execution (same handler pattern)
        var trends = await GetTrendsInternal(start, end, cancellationToken);

        return new SubscriptionAnalyticsDto
        {
            TotalSubscriptions = totalSubscriptions,
            ActiveSubscriptions = activeSubscriptionsCount,
            TrialSubscriptions = trialSubscriptionsCount,
            CancelledSubscriptions = cancelledSubscriptionsCount,
            MonthlyRecurringRevenue = mrr,
            AnnualRecurringRevenue = arr,
            ChurnRate = churnRate,
            AverageRevenuePerUser = arpu,
            SubscriptionsByPlan = subscriptionsByPlan,
            RevenueByPlan = revenueByPlan,
            Trends = trends.ToList()
        };
    }

    private async Task<IEnumerable<SubscriptionTrendDto>> GetTrendsInternal(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        var trends = new List<SubscriptionTrendDto>();
        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            var monthStart = new DateTime(currentDate.Year, currentDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // ✅ PERFORMANCE: Database'de count/sum yap
            var monthSubscriptionsCount = await _context.Set<UserSubscription>()
                .AsNoTracking()
                .CountAsync(us => us.CreatedAt >= monthStart && us.CreatedAt <= monthEnd, cancellationToken);

            var monthCancellations = await _context.Set<UserSubscription>()
                .AsNoTracking()
                .CountAsync(us => us.Status == SubscriptionStatus.Cancelled && 
                                 us.CancelledAt.HasValue &&
                                 us.CancelledAt >= monthStart && 
                                 us.CancelledAt <= monthEnd, cancellationToken);

            var activeAtMonthEnd = await _context.Set<UserSubscription>()
                .AsNoTracking()
                .CountAsync(us => us.Status == SubscriptionStatus.Active && us.EndDate > monthEnd, cancellationToken);

            var monthRevenue = await _context.Set<SubscriptionPayment>()
                .AsNoTracking()
                .Where(p => p.PaymentStatus == PaymentStatus.Completed &&
                           p.PaidAt.HasValue &&
                           p.PaidAt >= monthStart && 
                           p.PaidAt <= monthEnd)
                .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0;

            trends.Add(new SubscriptionTrendDto
            {
                Date = monthStart,
                NewSubscriptions = monthSubscriptionsCount,
                Cancellations = monthCancellations,
                ActiveSubscriptions = activeAtMonthEnd,
                Revenue = monthRevenue
            });

            currentDate = monthStart.AddMonths(1);
        }

        return trends;
    }
}
