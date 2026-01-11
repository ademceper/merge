using MediatR;
using Merge.Application.DTOs.Security;

namespace Merge.Application.Security.Queries.GetVerificationByOrderId;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetVerificationByOrderIdQuery(
    Guid OrderId
) : IRequest<OrderVerificationDto?>;
