using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Subscription;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using System.Text.Json;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Subscription.Commands.CreateSubscriptionPlan;

public class CreateSubscriptionPlanCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<CreateSubscriptionPlanCommandHandler> logger) : IRequestHandler<CreateSubscriptionPlanCommand, SubscriptionPlanDto>
{

    public async Task<SubscriptionPlanDto> Handle(CreateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating subscription plan. Name: {Name}, PlanType: {PlanType}, Price: {Price}",
            request.Name, request.PlanType, request.Price);

        var plan = SubscriptionPlan.Create(
            name: request.Name,
            description: request.Description,
            planType: request.PlanType,
            price: request.Price,
            durationDays: request.DurationDays,
            billingCycle: request.BillingCycle,
            maxUsers: request.MaxUsers,
            trialDays: request.TrialDays,
            setupFee: request.SetupFee,
            currency: request.Currency,
            features: request.Features != null ? JsonSerializer.Serialize(request.Features) : null,
            isActive: request.IsActive,
            displayOrder: request.DisplayOrder);

        await context.Set<SubscriptionPlan>().AddAsync(plan, cancellationToken);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        plan = await context.Set<SubscriptionPlan>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == plan.Id, cancellationToken);

        logger.LogInformation("Subscription plan created successfully. PlanId: {PlanId}, Name: {Name}",
            plan!.Id, plan.Name);

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
