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

namespace Merge.Application.Analytics.Commands.GenerateReport;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GenerateReportCommandHandler : IRequestHandler<GenerateReportCommand, ReportDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<GenerateReportCommandHandler> _logger;
    private readonly IMapper _mapper;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public GenerateReportCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<GenerateReportCommandHandler> logger,
        IMapper mapper)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ReportDto> Handle(GenerateReportCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating report. UserId: {UserId}, ReportType: {ReportType}, StartDate: {StartDate}, EndDate: {EndDate}",
            request.UserId, request.Type, request.StartDate, request.EndDate);
        
        // ✅ ARCHITECTURE: Transaction support for atomic report generation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
            var report = Report.Create(
                request.Name,
                request.Description,
                Enum.Parse<ReportType>(request.Type, true),
                request.UserId,
                request.StartDate,
                request.EndDate,
                request.Filters != null ? JsonSerializer.Serialize(request.Filters, JsonOptions) : null,
                Enum.Parse<ReportFormat>(request.Format, true));

            await _context.Set<Report>().AddAsync(report, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            report.MarkAsProcessing();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Generate report data based on type - MediatR ile diğer query handler'ları çağır
            object? reportData = Enum.Parse<ReportType>(request.Type, true) switch
            {
                ReportType.Sales => await _mediator.Send(new GetSalesAnalyticsQuery(request.StartDate, request.EndDate), cancellationToken),
                ReportType.Products => await _mediator.Send(new GetProductAnalyticsQuery(request.StartDate, request.EndDate), cancellationToken),
                ReportType.Customers => await _mediator.Send(new GetCustomerAnalyticsQuery(request.StartDate, request.EndDate), cancellationToken),
                ReportType.Financial => await _mediator.Send(new GetFinancialAnalyticsQuery(request.StartDate, request.EndDate), cancellationToken),
                _ => null
            };

            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
            report.Complete(JsonSerializer.Serialize(reportData, JsonOptions));

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ PERFORMANCE: Reload with Include for DTO mapping
            report = await _context.Set<Report>()
                .AsNoTracking()
                .Include(r => r.GeneratedByUser)
                .FirstOrDefaultAsync(r => r.Id == report.Id, cancellationToken);

            _logger.LogInformation("Report generated successfully. ReportId: {ReportId}, UserId: {UserId}", report!.Id, request.UserId);

            // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
            return _mapper.Map<ReportDto>(report);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "Rapor olusturma hatasi. UserId: {UserId}, ReportType: {ReportType}",
                request.UserId, request.Type);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

