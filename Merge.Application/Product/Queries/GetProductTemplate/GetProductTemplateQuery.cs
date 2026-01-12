using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetProductTemplate;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetProductTemplateQuery(
    Guid TemplateId
) : IRequest<ProductTemplateDto?>;
