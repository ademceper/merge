using MediatR;
using Merge.Application.DTOs.Security;

namespace Merge.Application.Security.Queries.GetBlockedPayments;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetBlockedPaymentsQuery() : IRequest<IEnumerable<PaymentFraudPreventionDto>>;
