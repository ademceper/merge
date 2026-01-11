using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Subscription;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Subscription.Queries.GetAllSubscriptionPlans;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAllSubscriptionPlansQueryHandler : IRequestHandler<GetAllSubscriptionPlansQuery, IEnumerable<SubscriptionPlanDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllSubscriptionPlansQueryHandler> _logger;

    public GetAllSubscriptionPlansQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetAllSubscriptionPlansQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<SubscriptionPlanDto>> Handle(GetAllSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query
        IQueryable<SubscriptionPlan> query = _context.Set<SubscriptionPlan>()
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

        // ✅ PERFORMANCE: Batch load subscriber counts for all plans
        var planIds = plans.Select(p => p.Id).ToList();
        var subscriberCounts = await _context.Set<UserSubscription>()
            .AsNoTracking()
            .Where(us => planIds.Contains(us.SubscriptionPlanId) && 
                        (us.Status == SubscriptionStatus.Active || us.Status == SubscriptionStatus.Trial))
            .GroupBy(us => us.SubscriptionPlanId)
            .Select(g => new { PlanId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.PlanId, x => x.Count, cancellationToken);

        var result = new List<SubscriptionPlanDto>();
        foreach (var plan in plans)
        {
            var dto = _mapper.Map<SubscriptionPlanDto>(plan);
            dto.SubscriberCount = subscriberCounts.TryGetValue(plan.Id, out var count) ? count : 0;
            result.Add(dto);
        }

        return result;
    }
}
