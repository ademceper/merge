using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces.Analytics;

namespace Merge.Application.Analytics.Commands.CreateReportSchedule;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CreateReportScheduleCommandHandler : IRequestHandler<CreateReportScheduleCommand, ReportScheduleDto>
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<CreateReportScheduleCommandHandler> _logger;

    public CreateReportScheduleCommandHandler(
        IAnalyticsService analyticsService,
        ILogger<CreateReportScheduleCommandHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<ReportScheduleDto> Handle(CreateReportScheduleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating report schedule. UserId: {UserId}, ReportType: {ReportType}, Frequency: {Frequency}",
            request.UserId, request.Type, request.Frequency);

        var dto = new CreateReportScheduleDto
        {
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Frequency = request.Frequency,
            DayOfWeek = request.DayOfWeek,
            DayOfMonth = request.DayOfMonth,
            TimeOfDay = request.TimeOfDay,
            Filters = request.Filters,
            Format = request.Format,
            EmailRecipients = request.EmailRecipients
        };

        return await _analyticsService.CreateReportScheduleAsync(dto, request.UserId, cancellationToken);
    }
}

