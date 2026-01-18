using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetComparisonMatrix;

public record GetComparisonMatrixQuery(
    Guid ComparisonId
) : IRequest<ComparisonMatrixDto>;
