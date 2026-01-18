using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetSizeGuidesByCategory;

public record GetSizeGuidesByCategoryQuery(
    Guid CategoryId
) : IRequest<IEnumerable<SizeGuideDto>>;
