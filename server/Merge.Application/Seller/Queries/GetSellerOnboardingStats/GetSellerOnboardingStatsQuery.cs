using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetSellerOnboardingStats;

public record GetSellerOnboardingStatsQuery() : IRequest<SellerOnboardingStatsDto>;
