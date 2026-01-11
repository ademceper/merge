using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetCommission;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCommissionQuery(
    Guid CommissionId
) : IRequest<SellerCommissionDto?>;
