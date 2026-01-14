using MediatR;
using Merge.Application.DTOs.Security;

namespace Merge.Application.Security.Queries.GetCheckByPaymentId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCheckByPaymentIdQuery(
    Guid PaymentId
) : IRequest<PaymentFraudPreventionDto?>;
