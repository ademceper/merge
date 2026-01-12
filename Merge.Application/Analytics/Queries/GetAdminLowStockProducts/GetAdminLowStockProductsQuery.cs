using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Analytics.Queries.GetAdminLowStockProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAdminLowStockProductsQuery(
    int Threshold
) : IRequest<IEnumerable<ProductDto>>;

