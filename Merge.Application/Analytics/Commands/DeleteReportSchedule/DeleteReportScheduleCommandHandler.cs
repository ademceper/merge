using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Commands.DeleteReportSchedule;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteReportScheduleCommandHandler : IRequestHandler<DeleteReportScheduleCommand, bool>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<DeleteReportScheduleCommandHandler> _logger;

    public DeleteReportScheduleCommandHandler(
        IAnalyticsService analyticsService,
        ILogger<DeleteReportScheduleCommandHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteReportScheduleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting report schedule. ScheduleId: {ScheduleId}, UserId: {UserId}",
            request.Id, request.UserId);

        return await _analyticsService.DeleteReportScheduleAsync(request.Id, request.UserId, cancellationToken);
    }
}

