using MediatR;
using Merge.Application.Common;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetReportSchedules;

public record GetReportSchedulesQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 0
) : IRequest<PagedResult<ReportScheduleDto>>;

