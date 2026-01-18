using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetAllProductTemplates;

public record GetAllProductTemplatesQuery(
    Guid? CategoryId = null,
    bool? IsActive = null
) : IRequest<IEnumerable<ProductTemplateDto>>;
