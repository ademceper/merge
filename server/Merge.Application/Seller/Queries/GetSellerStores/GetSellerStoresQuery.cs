using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Seller;
using Merge.Domain.Enums;

namespace Merge.Application.Seller.Queries.GetSellerStores;

public record GetSellerStoresQuery(
    Guid SellerId,
    EntityStatus? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<StoreDto>>;
