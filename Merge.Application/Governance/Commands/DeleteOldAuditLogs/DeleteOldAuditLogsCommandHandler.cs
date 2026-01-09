using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Governance.Commands.DeleteOldAuditLogs;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class DeleteOldAuditLogsCommandHandler : IRequestHandler<DeleteOldAuditLogsCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteOldAuditLogsCommandHandler> _logger;

    public DeleteOldAuditLogsCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteOldAuditLogsCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(DeleteOldAuditLogsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting old audit logs. DaysToKeep: {DaysToKeep}", request.DaysToKeep);

        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-request.DaysToKeep);

            // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
            var oldAudits = await _context.Set<AuditLog>()
                .Where(a => a.CreatedAt < cutoffDate && a.Severity != AuditSeverity.Critical)
                .ToListAsync(cancellationToken);

            _context.Set<AuditLog>().RemoveRange(oldAudits);
            // ✅ ARCHITECTURE: Bu işlem sadece silme olduğu için domain event yok
            // ✅ BOLUM 3.0: Outbox Pattern - Domain event'ler sadece entity değişikliklerinde oluşur
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Old audit logs deleted. DeletedCount: {DeletedCount}, DaysToKeep: {DaysToKeep}",
                oldAudits.Count, request.DaysToKeep);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex, "Error deleting old audit logs. DaysToKeep: {DaysToKeep}", request.DaysToKeep);
            throw;
        }
    }
}

