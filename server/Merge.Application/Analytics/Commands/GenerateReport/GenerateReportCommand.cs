using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Commands.GenerateReport;

public record GenerateReportCommand(
    Guid UserId,
    string Name,
    string Description,
    string Type,
    DateTime StartDate,
    DateTime EndDate,
    ReportFiltersDto? Filters,
    string Format
) : IRequest<ReportDto>;

