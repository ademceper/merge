using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Analytics.Commands.DeleteReport;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class DeleteReportCommandHandler : IRequestHandler<DeleteReportCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteReportCommandHandler> _logger;

    public DeleteReportCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<DeleteReportCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteReportCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting report. ReportId: {ReportId}, UserId: {UserId}", request.Id, request.UserId);
        
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted (Global Query Filter)
        var report = await _context.Set<Report>()
            .AsNoTracking()
            .Include(r => r.GeneratedByUser)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (report == null)
        {
            _logger.LogWarning("Report not found for deletion. ReportId: {ReportId}", request.Id);
            return false;
        }

        // ✅ SECURITY: Authorization check - Users can only delete their own reports unless Admin
        if (report.GeneratedBy != request.UserId)
        {
            _logger.LogWarning("Unauthorized report deletion attempt. ReportId: {ReportId}, UserId: {UserId}, ReportOwner: {ReportOwner}",
                request.Id, request.UserId, report.GeneratedBy);
            throw new UnauthorizedAccessException("Bu raporu silme yetkiniz yok.");
        }

        // Reload for update (AsNoTracking removed)
        report = await _context.Set<Report>()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (report == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        report.MarkAsDeleted();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Report deleted successfully. ReportId: {ReportId}", request.Id);
        return true;
    }
}

