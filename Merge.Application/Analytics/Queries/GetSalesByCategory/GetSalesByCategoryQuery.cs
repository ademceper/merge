using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetSalesByCategory;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSalesByCategoryQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<List<CategorySalesDto>>;

