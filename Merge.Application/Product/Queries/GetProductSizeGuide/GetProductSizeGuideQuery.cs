using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Queries.GetProductSizeGuide;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetProductSizeGuideQuery(
    Guid ProductId
) : IRequest<ProductSizeGuideDto?>;
