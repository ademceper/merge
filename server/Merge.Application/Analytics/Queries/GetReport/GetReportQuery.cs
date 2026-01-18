using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetReport;

public record GetReportQuery(
    Guid Id,
    Guid UserId
) : IRequest<ReportDto?>;

