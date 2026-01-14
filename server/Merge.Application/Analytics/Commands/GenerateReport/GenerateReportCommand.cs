using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Commands.GenerateReport;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
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

