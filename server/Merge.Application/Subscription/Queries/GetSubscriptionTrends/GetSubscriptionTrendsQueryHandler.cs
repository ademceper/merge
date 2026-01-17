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

namespace Merge.Application.Subscription.Queries.GetSubscriptionTrends;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSubscriptionTrendsQueryHandler(IDbContext context, ILogger<GetSubscriptionTrendsQueryHandler> logger) : IRequestHandler<GetSubscriptionTrendsQuery, IEnumerable<SubscriptionTrendDto>>
{

    public async Task<IEnumerable<SubscriptionTrendDto>> Handle(GetSubscriptionTrendsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        var trends = new List<SubscriptionTrendDto>();
        var currentDate = request.StartDate;

        while (currentDate <= request.EndDate)
        {
            var monthStart = new DateTime(currentDate.Year, currentDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // ✅ PERFORMANCE: Database'de count/sum yap
            var monthSubscriptionsCount = await context.Set<UserSubscription>()
                .AsNoTracking()
                .CountAsync(us => us.CreatedAt >= monthStart && us.CreatedAt <= monthEnd, cancellationToken);

            var monthCancellations = await context.Set<UserSubscription>()
                .AsNoTracking()
                .CountAsync(us => us.Status == SubscriptionStatus.Cancelled && 
                                 us.CancelledAt.HasValue &&
                                 us.CancelledAt >= monthStart && 
                                 us.CancelledAt <= monthEnd, cancellationToken);

            var activeAtMonthEnd = await context.Set<UserSubscription>()
                .AsNoTracking()
                .CountAsync(us => us.Status == SubscriptionStatus.Active && us.EndDate > monthEnd, cancellationToken);

            var monthRevenue = await context.Set<SubscriptionPayment>()
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
