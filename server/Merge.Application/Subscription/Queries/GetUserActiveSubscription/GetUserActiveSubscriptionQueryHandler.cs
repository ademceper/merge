using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Subscription;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Subscription.Queries.GetUserActiveSubscription;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetUserActiveSubscriptionQueryHandler(IDbContext context, IMapper mapper, ILogger<GetUserActiveSubscriptionQueryHandler> logger) : IRequestHandler<GetUserActiveSubscriptionQuery, UserSubscriptionDto?>
{

    public async Task<UserSubscriptionDto?> Handle(GetUserActiveSubscriptionQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query
        var subscription = await context.Set<UserSubscription>()
            .AsNoTracking()
            .Include(us => us.User)
            .Include(us => us.SubscriptionPlan)
            .Where(us => us.UserId == request.UserId && 
                        (us.Status == SubscriptionStatus.Active || us.Status == SubscriptionStatus.Trial) && 
                        us.EndDate > DateTime.UtcNow)
            .OrderByDescending(us => us.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (subscription == null)
        {
            return null;
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = mapper.Map<UserSubscriptionDto>(subscription);
        dto.DaysRemaining = subscription.EndDate > DateTime.UtcNow
            ? (int)(subscription.EndDate - DateTime.UtcNow).TotalDays
            : 0;

        // ✅ PERFORMANCE: Batch load recent payments
        var recentPayments = await context.Set<SubscriptionPayment>()
            .AsNoTracking()
            .Where(p => p.UserSubscriptionId == subscription.Id)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        dto.RecentPayments = mapper.Map<List<SubscriptionPaymentDto>>(recentPayments);

        return dto;
    }
}
