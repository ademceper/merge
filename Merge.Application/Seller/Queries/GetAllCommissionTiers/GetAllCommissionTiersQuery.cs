using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetAllCommissionTiers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAllCommissionTiersQuery() : IRequest<IEnumerable<CommissionTierDto>>;
