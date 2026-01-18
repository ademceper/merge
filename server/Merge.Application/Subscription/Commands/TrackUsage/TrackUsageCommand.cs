using MediatR;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Subscription.Commands.TrackUsage;

public record TrackUsageCommand(
    Guid UserSubscriptionId,
    string Feature,
    int Count = 1) : IRequest<SubscriptionUsageDto>;
