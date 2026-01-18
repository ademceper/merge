using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetAllProductBundles;

public record GetAllProductBundlesQuery(
    bool ActiveOnly = false
) : IRequest<IEnumerable<ProductBundleDto>>;
