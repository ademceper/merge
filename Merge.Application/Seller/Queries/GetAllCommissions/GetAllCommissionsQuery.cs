using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetAllCommissions;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public record GetAllCommissionsQuery(
    string? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<SellerCommissionDto>>;
