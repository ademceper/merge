using MediatR;
using Merge.Application.DTOs.Subscription;

namespace Merge.Application.Subscription.Queries.GetSubscriptionPayments;

public record GetSubscriptionPaymentsQuery(Guid UserSubscriptionId) : IRequest<IEnumerable<SubscriptionPaymentDto>>;
