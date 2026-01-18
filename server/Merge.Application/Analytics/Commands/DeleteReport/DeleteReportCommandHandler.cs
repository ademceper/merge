using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Analytics;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Commands.DeleteReport;

public class DeleteReportCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteReportCommandHandler> logger) : IRequestHandler<DeleteReportCommand, bool>
{

    public async Task<bool> Handle(DeleteReportCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting report. ReportId: {ReportId}, UserId: {UserId}", request.Id, request.UserId);
        
        var report = await context.Set<Report>()
            .AsNoTracking()
            .Include(r => r.GeneratedByUser)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (report is null)
        {
            logger.LogWarning("Report not found for deletion. ReportId: {ReportId}", request.Id);
            return false;
        }

        if (report.GeneratedBy != request.UserId)
        {
            logger.LogWarning("Unauthorized report deletion attempt. ReportId: {ReportId}, UserId: {UserId}, ReportOwner: {ReportOwner}",
                request.Id, request.UserId, report.GeneratedBy);
            throw new UnauthorizedAccessException("Bu raporu silme yetkiniz yok.");
        }

        // Reload for update (AsNoTracking removed)
        report = await context.Set<Report>()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (report is null) return false;

        report.MarkAsDeleted();
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Report deleted successfully. ReportId: {ReportId}", request.Id);
        return true;
    }
}

