using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetSellerCommissions;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSellerCommissionsQuery(
    Guid SellerId,
    string? Status = null
) : IRequest<IEnumerable<SellerCommissionDto>>;
