using MediatR;
using Merge.Application.DTOs.Security;
using Merge.Application.Common;

namespace Merge.Application.Security.Queries.GetAllVerifications;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAllVerificationsQuery(
    string? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<OrderVerificationDto>>;
