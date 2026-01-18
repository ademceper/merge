using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Analytics.Queries.GetAdminLowStockProducts;

public record GetAdminLowStockProductsQuery(
    int Threshold
) : IRequest<IEnumerable<ProductDto>>;

