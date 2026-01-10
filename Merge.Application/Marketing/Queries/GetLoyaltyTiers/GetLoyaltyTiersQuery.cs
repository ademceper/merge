using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Queries.GetLoyaltyTiers;

public record GetLoyaltyTiersQuery() : IRequest<IEnumerable<LoyaltyTierDto>>;
