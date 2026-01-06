using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Commands.CreateReportSchedule;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateReportScheduleCommand(
    Guid UserId,
    string Name,
    string Description,
    string Type,
    string Frequency,
    int DayOfWeek,
    int DayOfMonth,
    TimeSpan TimeOfDay,
    ReportFiltersDto? Filters,
    string Format,
    string EmailRecipients
) : IRequest<ReportScheduleDto>;

