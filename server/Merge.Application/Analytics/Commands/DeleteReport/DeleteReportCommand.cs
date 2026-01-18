using MediatR;

namespace Merge.Application.Analytics.Commands.DeleteReport;

public record DeleteReportCommand(
    Guid Id,
    Guid UserId
) : IRequest<bool>;

