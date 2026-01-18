using MediatR;

namespace Merge.Application.Analytics.Commands.ToggleReportSchedule;

public record ToggleReportScheduleCommand(
    Guid Id,
    bool IsActive,
    Guid UserId
) : IRequest<bool>;

