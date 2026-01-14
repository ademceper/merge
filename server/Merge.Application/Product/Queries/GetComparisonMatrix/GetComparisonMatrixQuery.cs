using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetComparisonMatrix;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetComparisonMatrixQuery(
    Guid ComparisonId
) : IRequest<ComparisonMatrixDto>;
