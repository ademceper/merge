using MediatR;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Subscription.Queries.GetUserActiveSubscription;

public record GetUserActiveSubscriptionQuery(Guid UserId) : IRequest<UserSubscriptionDto?>;
