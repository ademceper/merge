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

namespace Merge.Application.Subscription.Queries.GetSubscriptionPlanById;

public class GetSubscriptionPlanByIdQueryHandler(IDbContext context, IMapper mapper, ILogger<GetSubscriptionPlanByIdQueryHandler> logger) : IRequestHandler<GetSubscriptionPlanByIdQuery, SubscriptionPlanDto?>
{

    public async Task<SubscriptionPlanDto?> Handle(GetSubscriptionPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var plan = await context.Set<SubscriptionPlan>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (plan is null)
        {
            logger.LogWarning("Subscription plan not found. PlanId: {PlanId}", request.Id);
            return null;
        }

        var dto = mapper.Map<SubscriptionPlanDto>(plan);

        var subscriberCount = await context.Set<UserSubscription>()
            .AsNoTracking()
            .CountAsync(us => us.SubscriptionPlanId == plan.Id && 
                            (us.Status == SubscriptionStatus.Active || us.Status == SubscriptionStatus.Trial), 
                            cancellationToken);

        dto.SubscriberCount = subscriberCount;

        return dto;
    }
}
