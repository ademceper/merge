using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Seller;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Queries.GetAllSellerApplications;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 3.4: Pagination (ZORUNLU)
public record GetAllSellerApplicationsQuery(
    SellerApplicationStatus? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<SellerApplicationDto>>;
