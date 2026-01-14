using MediatR;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Subscription.Queries.GetUserSubscriptionById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetUserSubscriptionByIdQuery(Guid Id) : IRequest<UserSubscriptionDto?>;
