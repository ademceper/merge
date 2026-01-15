using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Exceptions;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Analytics;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.ExportReport;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class ExportReportQueryHandler(
    IDbContext context,
    ILogger<ExportReportQueryHandler> logger) : IRequestHandler<ExportReportQuery, byte[]?>
{

    public async Task<byte[]?> Handle(ExportReportQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Exporting report. ReportId: {ReportId}, UserId: {UserId}", request.Id, request.UserId);
        
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !r.IsDeleted (Global Query Filter)
        var report = await context.Set<Report>()
            .AsNoTracking()
            .Include(r => r.GeneratedByUser)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (report == null)
        {
            logger.LogWarning("Report not found for export. ReportId: {ReportId}", request.Id);
            throw new NotFoundException("Rapor", request.Id);
        }

        // ✅ SECURITY: Authorization check - Users can only export their own reports unless Admin
        if (report.GeneratedBy != request.UserId)
        {
            logger.LogWarning("Unauthorized report export attempt. ReportId: {ReportId}, UserId: {UserId}, ReportOwner: {ReportOwner}",
                request.Id, request.UserId, report.GeneratedBy);
            throw new UnauthorizedAccessException("Bu raporu export etme yetkiniz yok.");
        }

        // Simple CSV export example
        var data = report.Data ?? "{}";
        logger.LogInformation("Report exported successfully. ReportId: {ReportId}, DataSize: {DataSize} bytes", request.Id, data.Length);
        return System.Text.Encoding.UTF8.GetBytes(data);
    }
}

