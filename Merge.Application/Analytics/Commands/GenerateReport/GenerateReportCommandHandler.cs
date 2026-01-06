using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Commands.GenerateReport;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GenerateReportCommandHandler : IRequestHandler<GenerateReportCommand, ReportDto>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<GenerateReportCommandHandler> _logger;

    public GenerateReportCommandHandler(
        IAnalyticsService analyticsService,
        ILogger<GenerateReportCommandHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<ReportDto> Handle(GenerateReportCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating report. UserId: {UserId}, ReportType: {ReportType}, StartDate: {StartDate}, EndDate: {EndDate}",
            request.UserId, request.Type, request.StartDate, request.EndDate);

        var dto = new CreateReportDto
        {
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Filters = request.Filters,
            Format = request.Format
        };

        return await _analyticsService.GenerateReportAsync(dto, request.UserId, cancellationToken);
    }
}

