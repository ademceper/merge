using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetAllCommissionTiers;

public record GetAllCommissionTiersQuery() : IRequest<IEnumerable<CommissionTierDto>>;
