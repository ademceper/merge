using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Analytics.Queries.GetSalesAnalytics;
using Merge.Application.Analytics.Queries.GetProductAnalytics;
using Merge.Application.Analytics.Queries.GetCustomerAnalytics;
using Merge.Application.Analytics.Queries.GetFinancialAnalytics;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Analytics;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Commands.GenerateReport;

public class GenerateReportCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMediator mediator,
    ILogger<GenerateReportCommandHandler> logger,
    IMapper mapper) : IRequestHandler<GenerateReportCommand, ReportDto>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<ReportDto> Handle(GenerateReportCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Generating report. UserId: {UserId}, ReportType: {ReportType}, StartDate: {StartDate}, EndDate: {EndDate}",
            request.UserId, request.Type, request.StartDate, request.EndDate);
        
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var report = Report.Create(
                request.Name,
                request.Description,
                Enum.Parse<ReportType>(request.Type, true),
                request.UserId,
                request.StartDate,
                request.EndDate,
                request.Filters != null ? JsonSerializer.Serialize(request.Filters, JsonOptions) : null,
                Enum.Parse<ReportFormat>(request.Format, true));

            await context.Set<Report>().AddAsync(report, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            report.MarkAsProcessing();
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Generate report data based on type - MediatR ile diğer query handler'ları çağır
            object? reportData = Enum.Parse<ReportType>(request.Type, true) switch
            {
                ReportType.Sales => await mediator.Send(new GetSalesAnalyticsQuery(request.StartDate, request.EndDate), cancellationToken),
                ReportType.Products => await mediator.Send(new GetProductAnalyticsQuery(request.StartDate, request.EndDate), cancellationToken),
                ReportType.Customers => await mediator.Send(new GetCustomerAnalyticsQuery(request.StartDate, request.EndDate), cancellationToken),
                ReportType.Financial => await mediator.Send(new GetFinancialAnalyticsQuery(request.StartDate, request.EndDate), cancellationToken),
                _ => null
            };

            report.Complete(JsonSerializer.Serialize(reportData, JsonOptions));

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            report = await context.Set<Report>()
                .AsNoTracking()
                .Include(r => r.GeneratedByUser)
                .FirstOrDefaultAsync(r => r.Id == report.Id, cancellationToken);

            logger.LogInformation("Report generated successfully. ReportId: {ReportId}, UserId: {UserId}", report!.Id, request.UserId);

            return mapper.Map<ReportDto>(report);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Rapor olusturma hatasi. UserId: {UserId}, ReportType: {ReportType}",
                request.UserId, request.Type);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

