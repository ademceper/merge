using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using System.Text.Json;

namespace Merge.Application.Subscription.Commands.UpdateSubscriptionPlan;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UpdateSubscriptionPlanCommandHandler : IRequestHandler<UpdateSubscriptionPlanCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateSubscriptionPlanCommandHandler> _logger;

    public UpdateSubscriptionPlanCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdateSubscriptionPlanCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Updating subscription plan. PlanId: {PlanId}", request.Id);

        // ✅ NOT: AsNoTracking() YOK - Entity track edilmeli (update için)
        var plan = await _context.Set<SubscriptionPlan>()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (plan == null)
        {
            throw new NotFoundException("Abonelik planı", request.Id);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        var wasActive = plan.IsActive;
        
        plan.Update(
            name: request.Name,
            description: request.Description,
            price: request.Price,
            durationDays: request.DurationDays,
            trialDays: request.TrialDays,
            features: request.Features != null ? JsonSerializer.Serialize(request.Features) : null,
            isActive: request.IsActive,
            displayOrder: request.DisplayOrder,
            billingCycle: request.BillingCycle,
            maxUsers: request.MaxUsers,
            setupFee: request.SetupFee,
            currency: request.Currency);

        // ✅ BOLUM 1.1: Rich Domain Model - IsActive değiştiğinde domain method'ları çağır
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

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Subscription plan updated successfully. PlanId: {PlanId}, Name: {Name}",
            plan.Id, plan.Name);

        return true;
    }
}
