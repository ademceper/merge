using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Subscription.Commands.RenewSubscription;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RenewSubscriptionCommandHandler : IRequestHandler<RenewSubscriptionCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RenewSubscriptionCommandHandler> _logger;

    public RenewSubscriptionCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RenewSubscriptionCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(RenewSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Renewing subscription. SubscriptionId: {SubscriptionId}", request.SubscriptionId);

        // ✅ NOT: AsNoTracking() YOK - Entity track edilmeli (update için)
        var subscription = await _context.Set<UserSubscription>()
            .Include(us => us.SubscriptionPlan)
            .FirstOrDefaultAsync(us => us.Id == request.SubscriptionId, cancellationToken);

        if (subscription == null)
        {
            throw new NotFoundException("Abonelik", request.SubscriptionId);
        }

        var plan = subscription.SubscriptionPlan;
        if (plan == null)
        {
            throw new NotFoundException("Abonelik planı", subscription.SubscriptionPlanId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        subscription.Renew(plan);

        // Create payment for renewal
        var billingPeriodStart = subscription.NextBillingDate ?? subscription.EndDate.AddDays(-plan.DurationDays);
        var billingPeriodEnd = subscription.EndDate;
        
        var payment = SubscriptionPayment.Create(
            subscription: subscription,
            amount: plan.Price,
            billingPeriodStart: billingPeriodStart,
            billingPeriodEnd: billingPeriodEnd);

        await _context.Set<SubscriptionPayment>().AddAsync(payment, cancellationToken);

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Subscription renewed successfully. SubscriptionId: {SubscriptionId}, RenewalCount: {RenewalCount}",
            subscription.Id, subscription.RenewalCount);

        return true;
    }
}
