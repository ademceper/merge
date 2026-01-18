using MediatR;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Common;

namespace Merge.Application.ML.Queries.ForecastDemandForCategory;

public record ForecastDemandForCategoryQuery(
    Guid CategoryId,
    int ForecastDays = 30,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<DemandForecastDto>>;
