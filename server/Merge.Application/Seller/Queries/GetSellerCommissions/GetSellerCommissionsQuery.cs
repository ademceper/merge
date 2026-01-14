using MediatR;
using Merge.Application.DTOs.Seller;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Queries.GetSellerCommissions;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ ARCHITECTURE: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
public record GetSellerCommissionsQuery(
    Guid SellerId,
    CommissionStatus? Status = null
) : IRequest<IEnumerable<SellerCommissionDto>>;
