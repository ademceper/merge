using MediatR;
using Merge.Application.DTOs.Security;

namespace Merge.Application.Security.Queries.GetBlockedPayments;

public record GetBlockedPaymentsQuery() : IRequest<IEnumerable<PaymentFraudPreventionDto>>;
