using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Analytics.Commands.DeleteReportSchedule;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteReportScheduleCommandHandler : IRequestHandler<DeleteReportScheduleCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteReportScheduleCommandHandler> _logger;

    public DeleteReportScheduleCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteReportScheduleCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteReportScheduleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting report schedule. ScheduleId: {ScheduleId}, UserId: {UserId}",
            request.Id, request.UserId);

        // ✅ PERFORMANCE: Removed manual !s.IsDeleted check (Global Query Filter handles it)
        var schedule = await _context.Set<ReportSchedule>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (schedule == null) return false;

        // ✅ SECURITY: Authorization check - Users can only delete their own schedules unless Admin
        if (schedule.OwnerId != request.UserId)
        {
            throw new UnauthorizedAccessException("Bu rapor zamanlamasını silme yetkiniz yok.");
        }

        // Reload for update (AsNoTracking removed)
        schedule = await _context.Set<ReportSchedule>()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (schedule == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        schedule.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

