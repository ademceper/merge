using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetAdminTopProducts;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAdminTopProductsQuery(
    int Count
) : IRequest<IEnumerable<AdminTopProductDto>>;

