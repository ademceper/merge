using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Queries.GetOrganizationCreditTerms;

public record GetOrganizationCreditTermsQuery(
    Guid OrganizationId,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<CreditTermDto>>;

