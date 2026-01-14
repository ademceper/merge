using MediatR;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Subscription.Commands.TrackUsage;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record TrackUsageCommand(
    Guid UserSubscriptionId,
    string Feature,
    int Count = 1) : IRequest<SubscriptionUsageDto>;
