using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetProductBundleById;

public record GetProductBundleByIdQuery(
    Guid Id
) : IRequest<ProductBundleDto?>;
