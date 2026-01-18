using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetPopularProductTemplates;

public record GetPopularProductTemplatesQuery(
    int Limit = 10
) : IRequest<IEnumerable<ProductTemplateDto>>;
