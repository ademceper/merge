using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using AutoMapper;

namespace Merge.Application.Analytics.Commands.CreateReportSchedule;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateReportScheduleCommandHandler : IRequestHandler<CreateReportScheduleCommand, ReportScheduleDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateReportScheduleCommandHandler> _logger;
    private readonly IMapper _mapper;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public CreateReportScheduleCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<CreateReportScheduleCommandHandler> logger,
        IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ReportScheduleDto> Handle(CreateReportScheduleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating report schedule. UserId: {UserId}, ReportType: {ReportType}, Frequency: {Frequency}",
            request.UserId, request.Type, request.Frequency);
        
        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var schedule = ReportSchedule.Create(
            request.Name,
            request.Description,
            Enum.Parse<ReportType>(request.Type, true),
            request.UserId,
            Enum.Parse<ReportFrequency>(request.Frequency, true),
            request.TimeOfDay,
            request.Filters != null ? JsonSerializer.Serialize(request.Filters, JsonOptions) : null,
            Enum.Parse<ReportFormat>(request.Format, true),
            request.EmailRecipients,
            request.DayOfWeek,
            request.DayOfMonth);

        await _context.Set<ReportSchedule>().AddAsync(schedule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Report schedule created successfully. ScheduleId: {ScheduleId}, UserId: {UserId}", schedule.Id, request.UserId);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<ReportScheduleDto>(schedule);
    }
}

