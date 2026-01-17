using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Subscription;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Subscription.Queries.GetAllSubscriptionPlans;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAllSubscriptionPlansQueryHandler(IDbContext context, IMapper mapper, ILogger<GetAllSubscriptionPlansQueryHandler> logger) : IRequestHandler<GetAllSubscriptionPlansQuery, IEnumerable<SubscriptionPlanDto>>
{

    public async Task<IEnumerable<SubscriptionPlanDto>> Handle(GetAllSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query
        IQueryable<SubscriptionPlan> query = context.Set<SubscriptionPlan>()
            .AsNoTracking();

        if (request.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == request.IsActive.Value);
        }

        var plans = await query
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Price)
            .ToListAsync(cancellationToken);

        if (!plans.Any())
        {
            return Enumerable.Empty<SubscriptionPlanDto>();
        }

        // ✅ PERFORMANCE: Batch load subscriber counts for all plans (subquery ile)
        var planIdsSubquery = from p in query select p.Id;
        var subscriberCounts = await context.Set<UserSubscription>()
            .AsNoTracking()
            .Where(us => planIdsSubquery.Contains(us.SubscriptionPlanId) && 
                        (us.Status == SubscriptionStatus.Active || us.Status == SubscriptionStatus.Trial))
            .GroupBy(us => us.SubscriptionPlanId)
            .Select(g => new { PlanId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PlanId, x => x.Count, cancellationToken);

        var result = new List<SubscriptionPlanDto>();
        foreach (var plan in plans)
        {
            var dto = mapper.Map<SubscriptionPlanDto>(plan);
            dto.SubscriberCount = subscriberCounts.TryGetValue(plan.Id, out var count) ? count : 0;
            result.Add(dto);
        }

        return result;
    }
}
