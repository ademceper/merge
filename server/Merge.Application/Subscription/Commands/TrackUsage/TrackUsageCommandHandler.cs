using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Subscription;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Subscription.Commands.TrackUsage;

public class TrackUsageCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<TrackUsageCommandHandler> logger) : IRequestHandler<TrackUsageCommand, SubscriptionUsageDto>
{

    public async Task<SubscriptionUsageDto> Handle(TrackUsageCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Tracking subscription usage. SubscriptionId: {SubscriptionId}, Feature: {Feature}, Count: {Count}",
            request.UserSubscriptionId, request.Feature, request.Count);

        var subscription = await context.Set<UserSubscription>()
            .FirstOrDefaultAsync(us => us.Id == request.UserSubscriptionId, cancellationToken);

        if (subscription == null)
        {
            throw new NotFoundException("Abonelik", request.UserSubscriptionId);
        }

        var periodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);

        var usage = await context.Set<SubscriptionUsage>()
            .Include(u => u.UserSubscription)
            .FirstOrDefaultAsync(u => u.UserSubscriptionId == request.UserSubscriptionId &&
                                     u.Feature == request.Feature &&
                                     u.PeriodStart == periodStart, cancellationToken);

        if (usage == null)
        {
            usage = SubscriptionUsage.Create(
                subscription: subscription,
                feature: request.Feature,
                periodStart: periodStart,
                periodEnd: periodEnd);
            
            await context.Set<SubscriptionUsage>().AddAsync(usage, cancellationToken);
        }

        usage.IncrementUsage(request.Count);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<SubscriptionUsageDto>(usage);
    }
}
