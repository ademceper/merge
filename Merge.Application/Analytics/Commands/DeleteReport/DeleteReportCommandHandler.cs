using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Commands.DeleteReport;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteReportCommandHandler : IRequestHandler<DeleteReportCommand, bool>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<DeleteReportCommandHandler> _logger;

    public DeleteReportCommandHandler(
        IAnalyticsService analyticsService,
        ILogger<DeleteReportCommandHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteReportCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting report. ReportId: {ReportId}, UserId: {UserId}", request.Id, request.UserId);

        return await _analyticsService.DeleteReportAsync(request.Id, request.UserId, cancellationToken);
    }
}

