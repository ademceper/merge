using MediatR;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;

namespace Merge.Application.Marketing.Queries.GetMyReferrals;

public record GetMyReferralsQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<ReferralDto>>;
