using MediatR;

namespace Merge.Application.Analytics.Commands.ToggleReportSchedule;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ToggleReportScheduleCommand(
    Guid Id,
    bool IsActive,
    Guid UserId
) : IRequest<bool>;

