using MediatR;
using Merge.Application.DTOs.Security;

namespace Merge.Application.Security.Queries.GetPendingVerifications;

public record GetPendingVerificationsQuery() : IRequest<IEnumerable<OrderVerificationDto>>;
