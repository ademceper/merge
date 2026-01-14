using MediatR;

namespace Merge.Application.Analytics.Commands.DeleteReportSchedule;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteReportScheduleCommand(
    Guid Id,
    Guid UserId
) : IRequest<bool>;

