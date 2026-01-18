using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetProductComparisonById;

public record GetProductComparisonByIdQuery(
    Guid Id
) : IRequest<ProductComparisonDto?>;
