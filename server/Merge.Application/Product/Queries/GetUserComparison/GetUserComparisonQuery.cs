using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetUserComparison;

public record GetUserComparisonQuery(
    Guid UserId
) : IRequest<ProductComparisonDto>;
