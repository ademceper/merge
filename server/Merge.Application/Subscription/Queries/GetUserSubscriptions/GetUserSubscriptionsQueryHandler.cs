using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Subscription;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Subscription.Queries.GetUserSubscriptions;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public class GetUserSubscriptionsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetUserSubscriptionsQueryHandler> logger) : IRequestHandler<GetUserSubscriptionsQuery, PagedResult<UserSubscriptionDto>>
{

    public async Task<PagedResult<UserSubscriptionDto>> Handle(GetUserSubscriptionsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        var pageSize = request.PageSize > 100 ? 100 : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        // ✅ PERFORMANCE: AsNoTracking for read-only query
        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        IQueryable<UserSubscription> query = context.Set<UserSubscription>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(us => us.User)
            .Include(us => us.SubscriptionPlan)
            .Where(us => us.UserId == request.UserId);

        if (request.Status.HasValue)
        {
            query = query.Where(us => us.Status == request.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Pagination uygula
        var subscriptionIds = await query
            .OrderByDescending(us => us.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(us => us.Id)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: AsSplitQuery to prevent Cartesian Explosion (multiple Includes)
        var subscriptions = await context.Set<UserSubscription>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(us => us.User)
            .Include(us => us.SubscriptionPlan)
            .Where(us => subscriptionIds.Contains(us.Id))
            .OrderByDescending(us => us.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load recent payments for all subscriptions
        var recentPaymentsDict = await context.Set<SubscriptionPayment>()
            .AsNoTracking()
            .Where(p => subscriptionIds.Contains(p.UserSubscriptionId))
            .OrderByDescending(p => p.CreatedAt)
            .GroupBy(p => p.UserSubscriptionId)
            .Select(g => new
            {
                UserSubscriptionId = g.Key,
                Payments = g.Take(5).ToList()
            })
            .ToDictionaryAsync(x => x.UserSubscriptionId, x => x.Payments, cancellationToken);

        var result = new List<UserSubscriptionDto>();
        foreach (var subscription in subscriptions)
        {
            var dto = mapper.Map<UserSubscriptionDto>(subscription);
            dto.DaysRemaining = subscription.EndDate > DateTime.UtcNow
                ? (int)(subscription.EndDate - DateTime.UtcNow).TotalDays
                : 0;
            
            if (recentPaymentsDict.TryGetValue(subscription.Id, out var payments))
            {
                dto.RecentPayments = mapper.Map<List<SubscriptionPaymentDto>>(payments);
            }
            else
            {
                dto.RecentPayments = new List<SubscriptionPaymentDto>();
            }
            
            result.Add(dto);
        }

        return new PagedResult<UserSubscriptionDto>
        {
            Items = result,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
