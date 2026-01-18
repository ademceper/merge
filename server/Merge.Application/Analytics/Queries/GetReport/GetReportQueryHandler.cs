using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Analytics;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetReport;

public class GetReportQueryHandler(
    IDbContext context,
    ILogger<GetReportQueryHandler> logger,
    IMapper mapper) : IRequestHandler<GetReportQuery, ReportDto?>
{

    public async Task<ReportDto?> Handle(GetReportQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching report. ReportId: {ReportId}, UserId: {UserId}", request.Id, request.UserId);

        var report = await context.Set<Report>()
            .AsNoTracking()
            .Include(r => r.GeneratedByUser)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (report == null)
        {
            logger.LogWarning("Report not found. ReportId: {ReportId}", request.Id);
            return null;
        }

        // Bu kontrol controller'da yapılıyor, burada sadece data getiriyoruz

        return mapper.Map<ReportDto>(report);
    }
}

