using MediatR;

namespace Merge.Application.Analytics.Commands.DeleteReportSchedule;

public record DeleteReportScheduleCommand(
    Guid Id,
    Guid UserId
) : IRequest<bool>;

