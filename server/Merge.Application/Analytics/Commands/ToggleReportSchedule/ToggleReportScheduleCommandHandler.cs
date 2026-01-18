using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Analytics;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Commands.ToggleReportSchedule;

public class ToggleReportScheduleCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<ToggleReportScheduleCommandHandler> logger) : IRequestHandler<ToggleReportScheduleCommand, bool>
{

    public async Task<bool> Handle(ToggleReportScheduleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Toggling report schedule. ScheduleId: {ScheduleId}, IsActive: {IsActive}, UserId: {UserId}",
            request.Id, request.IsActive, request.UserId);

        var schedule = await context.Set<ReportSchedule>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (schedule is null) return false;

        if (schedule.OwnerId != request.UserId)
        {
            throw new UnauthorizedAccessException("Bu rapor zamanlamasını değiştirme yetkiniz yok.");
        }

        // Reload for update (AsNoTracking removed)
        schedule = await context.Set<ReportSchedule>()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (schedule is null) return false;

        if (request.IsActive)
            schedule.Activate();
        else
            schedule.Deactivate();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

