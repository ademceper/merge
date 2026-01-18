using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Seller.Queries.GetSellerProducts;

public record GetSellerProductsQuery(
    Guid SellerId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProductDto>>;
