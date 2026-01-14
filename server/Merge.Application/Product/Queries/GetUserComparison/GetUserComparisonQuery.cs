using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetUserComparison;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetUserComparisonQuery(
    Guid UserId
) : IRequest<ProductComparisonDto>;
