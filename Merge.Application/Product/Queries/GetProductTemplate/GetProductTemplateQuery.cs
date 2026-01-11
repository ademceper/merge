using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Queries.GetProductTemplate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetProductTemplateQuery(
    Guid TemplateId
) : IRequest<ProductTemplateDto?>;
