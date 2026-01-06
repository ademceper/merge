using MediatR;

namespace Merge.Application.Analytics.Commands.DeleteReport;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteReportCommand(
    Guid Id,
    Guid UserId
) : IRequest<bool>;

