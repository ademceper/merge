using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Queries.GetAllProductTemplates;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAllProductTemplatesQuery(
    Guid? CategoryId = null,
    bool? IsActive = null
) : IRequest<IEnumerable<ProductTemplateDto>>;
