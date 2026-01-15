using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Analytics;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Commands.DeleteReportSchedule;

public class DeleteReportScheduleCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteReportScheduleCommandHandler> logger) : IRequestHandler<DeleteReportScheduleCommand, bool>
{

    public async Task<bool> Handle(DeleteReportScheduleCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting report schedule. ScheduleId: {ScheduleId}, UserId: {UserId}",
            request.Id, request.UserId);

        var schedule = await context.Set<ReportSchedule>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (schedule == null) return false;

        if (schedule.OwnerId != request.UserId)
        {
            throw new UnauthorizedAccessException("Bu rapor zamanlamasını silme yetkiniz yok.");
        }

        schedule = await context.Set<ReportSchedule>()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (schedule == null) return false;

        schedule.MarkAsDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

