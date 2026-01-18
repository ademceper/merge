using MediatR;
using Merge.Application.DTOs.Security;

namespace Merge.Application.Security.Queries.GetVerificationByOrderId;

public record GetVerificationByOrderIdQuery(
    Guid OrderId
) : IRequest<OrderVerificationDto?>;
