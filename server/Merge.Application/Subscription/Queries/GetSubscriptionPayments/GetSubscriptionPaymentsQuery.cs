using MediatR;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Subscription.Queries.GetSubscriptionPayments;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSubscriptionPaymentsQuery(Guid UserSubscriptionId) : IRequest<IEnumerable<SubscriptionPaymentDto>>;
