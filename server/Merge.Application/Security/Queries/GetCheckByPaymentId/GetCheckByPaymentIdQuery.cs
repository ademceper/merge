using MediatR;
using Merge.Application.DTOs.Security;

namespace Merge.Application.Security.Queries.GetCheckByPaymentId;

public record GetCheckByPaymentIdQuery(
    Guid PaymentId
) : IRequest<PaymentFraudPreventionDto?>;
