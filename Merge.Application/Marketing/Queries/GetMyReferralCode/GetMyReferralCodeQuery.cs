using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetMyReferralCode;

public record GetMyReferralCodeQuery(
    Guid UserId) : IRequest<ReferralCodeDto>;
