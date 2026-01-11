using MediatR;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Subscription.Queries.GetUserActiveSubscription;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetUserActiveSubscriptionQuery(Guid UserId) : IRequest<UserSubscriptionDto?>;
