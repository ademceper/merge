using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Seller;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Queries.GetAllCommissions;

public record GetAllCommissionsQuery(
    CommissionStatus? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<SellerCommissionDto>>;
