using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using System.Text.Json;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Subscription.Commands.UpdateSubscriptionPlan;

public class UpdateSubscriptionPlanCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<UpdateSubscriptionPlanCommandHandler> logger) : IRequestHandler<UpdateSubscriptionPlanCommand, bool>
{

    public async Task<bool> Handle(UpdateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating subscription plan. PlanId: {PlanId}", request.Id);

        var plan = await context.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (plan is null)
        {
            throw new NotFoundException("Abonelik planı", request.Id);
        }

        var wasActive = plan.IsActive;
        
        plan.Update(
            name: request.Name,
            description: request.Description,
            price: request.Price,
            durationDays: request.DurationDays,
            trialDays: request.TrialDays,
            features: request.Features is not null ? JsonSerializer.Serialize(request.Features) : null,
            isActive: request.IsActive,
            displayOrder: request.DisplayOrder,
            billingCycle: request.BillingCycle,
            maxUsers: request.MaxUsers,
            setupFee: request.SetupFee,
            currency: request.Currency);

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value && !wasActive)
            {
                plan.Activate();
            }
            else if (!request.IsActive.Value && wasActive)
            {
                plan.Deactivate();
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Subscription plan updated successfully. PlanId: {PlanId}, Name: {Name}",
            plan.Id, plan.Name);

        return true;
    }
}
