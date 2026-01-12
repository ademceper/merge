using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetAllProductBundles;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAllProductBundlesQuery(
    bool ActiveOnly = false
) : IRequest<IEnumerable<ProductBundleDto>>;
