using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetReferralStats;

public record GetReferralStatsQuery(
    Guid UserId) : IRequest<ReferralStatsDto>;
