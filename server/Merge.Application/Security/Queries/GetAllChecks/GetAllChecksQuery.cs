using MediatR;
using Merge.Application.DTOs.Security;
using Merge.Application.Common;

namespace Merge.Application.Security.Queries.GetAllChecks;

public record GetAllChecksQuery(
    string? Status = null,
    bool? IsBlocked = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<PaymentFraudPreventionDto>>;
