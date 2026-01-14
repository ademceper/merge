using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetSizeGuidesByCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSizeGuidesByCategoryQuery(
    Guid CategoryId
) : IRequest<IEnumerable<SizeGuideDto>>;
