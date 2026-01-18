using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Subscription.Commands.DeleteSubscriptionPlan;

public class DeleteSubscriptionPlanCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<DeleteSubscriptionPlanCommandHandler> logger) : IRequestHandler<DeleteSubscriptionPlanCommand, bool>
{

    public async Task<bool> Handle(DeleteSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting subscription plan. PlanId: {PlanId}", request.Id);

        var plan = await context.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (plan is null)
        {
            throw new NotFoundException("Abonelik planı", request.Id);
        }

        plan.Delete();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Subscription plan deleted successfully. PlanId: {PlanId}", plan.Id);

        return true;
    }
}
