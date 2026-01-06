using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Commands.ToggleReportSchedule;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ToggleReportScheduleCommandHandler : IRequestHandler<ToggleReportScheduleCommand, bool>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<ToggleReportScheduleCommandHandler> _logger;

    public ToggleReportScheduleCommandHandler(
        IAnalyticsService analyticsService,
        ILogger<ToggleReportScheduleCommandHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<bool> Handle(ToggleReportScheduleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Toggling report schedule. ScheduleId: {ScheduleId}, IsActive: {IsActive}, UserId: {UserId}",
            request.Id, request.IsActive, request.UserId);

        return await _analyticsService.ToggleReportScheduleAsync(request.Id, request.IsActive, request.UserId, cancellationToken);
    }
}

