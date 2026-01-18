using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetProductTemplate;

public record GetProductTemplateQuery(
    Guid TemplateId
) : IRequest<ProductTemplateDto?>;
