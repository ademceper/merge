using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetLoyaltyStats;

public record GetLoyaltyStatsQuery() : IRequest<LoyaltyStatsDto>;
