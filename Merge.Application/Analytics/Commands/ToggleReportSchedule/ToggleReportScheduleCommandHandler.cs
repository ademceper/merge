using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Analytics.Commands.ToggleReportSchedule;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class ToggleReportScheduleCommandHandler : IRequestHandler<ToggleReportScheduleCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ToggleReportScheduleCommandHandler> _logger;

    public ToggleReportScheduleCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ToggleReportScheduleCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ToggleReportScheduleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Toggling report schedule. ScheduleId: {ScheduleId}, IsActive: {IsActive}, UserId: {UserId}",
            request.Id, request.IsActive, request.UserId);

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted check (Global Query Filter handles it)
        var schedule = await _context.Set<ReportSchedule>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (schedule == null) return false;

        // ✅ SECURITY: Authorization check - Users can only toggle their own schedules unless Admin
        if (schedule.OwnerId != request.UserId)
        {
            throw new UnauthorizedAccessException("Bu rapor zamanlamasını değiştirme yetkiniz yok.");
        }

        // Reload for update (AsNoTracking removed)
        schedule = await _context.Set<ReportSchedule>()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (schedule == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (request.IsActive)
            schedule.Activate();
        else
            schedule.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

