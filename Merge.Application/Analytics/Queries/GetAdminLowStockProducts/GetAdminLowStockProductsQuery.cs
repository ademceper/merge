using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Analytics.Queries.GetAdminLowStockProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAdminLowStockProductsQuery(
    int Threshold
) : IRequest<IEnumerable<ProductDto>>;

