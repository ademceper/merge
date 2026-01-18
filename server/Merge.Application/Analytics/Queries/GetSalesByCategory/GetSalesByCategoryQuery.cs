using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetSalesByCategory;

public record GetSalesByCategoryQuery(
    DateTime StartDate,
    DateTime EndDate
) : IRequest<List<CategorySalesDto>>;

