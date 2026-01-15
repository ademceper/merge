using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using Merge.Domain.SharedKernel;

namespace Merge.Application.Governance.Commands.DeleteOldAuditLogs;

public class DeleteOldAuditLogsCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<DeleteOldAuditLogsCommandHandler> logger) : IRequestHandler<DeleteOldAuditLogsCommand>
{

    public async Task Handle(DeleteOldAuditLogsCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Deleting old audit logs. DaysToKeep: {DaysToKeep}", request.DaysToKeep);

        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-request.DaysToKeep);

            var oldAudits = await context.Set<AuditLog>()
                .Where(a => a.CreatedAt < cutoffDate && a.Severity != AuditSeverity.Critical)
                .ToListAsync(cancellationToken);

            context.Set<AuditLog>().RemoveRange(oldAudits);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Old audit logs deleted. DeletedCount: {DeletedCount}, DaysToKeep: {DaysToKeep}",
                oldAudits.Count, request.DaysToKeep);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting old audit logs. DaysToKeep: {DaysToKeep}", request.DaysToKeep);
            throw;
        }
    }
}
