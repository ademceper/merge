using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetFinancialSummaries;

public class GetFinancialSummariesQueryHandler(
    IDbContext context,
    ILogger<GetFinancialSummariesQueryHandler> logger,
    IOptions<AnalyticsSettings> settings) : IRequestHandler<GetFinancialSummariesQuery, List<FinancialSummaryDto>>
{

    public async Task<List<FinancialSummaryDto>> Handle(GetFinancialSummariesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching financial summaries. StartDate: {StartDate}, EndDate: {EndDate}, Period: {Period}",
            request.StartDate, request.EndDate, request.Period);

        List<FinancialSummaryDto> summaries;

        if (request.Period == "daily")
        {
            summaries = await context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                      o.CreatedAt >= request.StartDate &&
                      o.CreatedAt <= request.EndDate)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new FinancialSummaryDto(
                    g.Key,
                    g.Sum(o => o.TotalAmount),
                    g.Sum(o => o.TotalAmount * settings.Value.DefaultCostPercentage),
                    g.Sum(o => o.TotalAmount * settings.Value.DefaultProfitPercentage),
                    (int)(settings.Value.DefaultProfitPercentage * 100),
                    g.Count()
                ))
                .OrderBy(s => s.Period)
                .ToListAsync(cancellationToken);
        }
        else if (request.Period == "weekly")
        {
            // ISOWeek.GetWeekOfYear client-side function olduğu için raw SQL kullanıyoruz
            var costPercentage = settings.Value.DefaultCostPercentage;
            var profitPercentage = settings.Value.DefaultProfitPercentage;
            var profitMargin = (int)(profitPercentage * 100);
            
            var paymentStatusValue = (int)PaymentStatus.Completed;
            summaries = await context.Database
                .SqlQueryRaw<FinancialSummaryDto>(@"
                    SELECT 
                        DATE_TRUNC('week', ""CreatedAt"")::date AS ""Period"",
                        SUM(""TotalAmount"") AS ""TotalRevenue"",
                        SUM(""TotalAmount"" * {2}) AS ""TotalCosts"",
                        SUM(""TotalAmount"" * {3}) AS ""NetProfit"",
                        {4} AS ""ProfitMargin"",
                        COUNT(*) AS ""TotalOrders""
                    FROM ""Orders""
                    WHERE ""PaymentStatus"" = {5}
                      AND ""CreatedAt"" >= {0}
                      AND ""CreatedAt"" <= {1}
                    GROUP BY DATE_TRUNC('week', ""CreatedAt"")
                    ORDER BY ""Period""
                ", request.StartDate, request.EndDate, costPercentage, profitPercentage, profitMargin, paymentStatusValue)
                .ToListAsync(cancellationToken);
        }
        else if (request.Period == "monthly")
        {
            summaries = await context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                      o.CreatedAt >= request.StartDate &&
                      o.CreatedAt <= request.EndDate)
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new FinancialSummaryDto(
                    new DateTime(g.Key.Year, g.Key.Month, 1),
                    g.Sum(o => o.TotalAmount),
                    g.Sum(o => o.TotalAmount * settings.Value.DefaultCostPercentage),
                    g.Sum(o => o.TotalAmount * settings.Value.DefaultProfitPercentage),
                    (int)(settings.Value.DefaultProfitPercentage * 100),
                    g.Count()
                ))
                .OrderBy(s => s.Period)
                .ToListAsync(cancellationToken);
        }
        else
        {
            summaries = new List<FinancialSummaryDto>(0); // Pre-allocate with known capacity (0)
        }

        return summaries;
    }
}

