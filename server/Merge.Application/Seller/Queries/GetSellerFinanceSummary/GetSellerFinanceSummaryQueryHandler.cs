using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.Seller.Queries.GetSellerBalance;
using Merge.Application.Seller.Queries.GetSellerTransactions;
using Merge.Application.Seller.Queries.GetSellerInvoices;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Seller.Queries.GetSellerFinanceSummary;

public class GetSellerFinanceSummaryQueryHandler(IDbContext context, IMediator mediator, ILogger<GetSellerFinanceSummaryQueryHandler> logger, IOptions<SellerSettings> sellerSettings) : IRequestHandler<GetSellerFinanceSummaryQuery, SellerFinanceSummaryDto>
{
    private readonly SellerSettings sellerConfig = sellerSettings.Value;

    public async Task<SellerFinanceSummaryDto> Handle(GetSellerFinanceSummaryQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting seller finance summary. SellerId: {SellerId}, StartDate: {StartDate}, EndDate: {EndDate}",
            request.SellerId, request.StartDate, request.EndDate);

        var startDate = request.StartDate ?? DateTime.UtcNow.AddDays(-sellerConfig.DefaultStatsPeriodDays);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        // Get balance using existing query
        var balanceQuery = new Queries.GetSellerBalance.GetSellerBalanceQuery(request.SellerId);
        var balance = await mediator.Send(balanceQuery, cancellationToken);

        // Get recent transactions using existing query
        var transactionsQuery = new Queries.GetSellerTransactions.GetSellerTransactionsQuery(
            request.SellerId, null, startDate, endDate, 1, sellerConfig.RecentItemsLimit);
        var transactions = await mediator.Send(transactionsQuery, cancellationToken);

        // Get recent invoices using existing query
        var invoicesQuery = new Queries.GetSellerInvoices.GetSellerInvoicesQuery(
            request.SellerId, null, 1, sellerConfig.RecentItemsLimit);
        var invoices = await mediator.Send(invoicesQuery, cancellationToken);

        // Earnings by month
        var earningsByMonth = await context.Set<SellerCommission>()
            .AsNoTracking()
            .Where(sc => sc.SellerId == request.SellerId &&
                  sc.CreatedAt >= startDate &&
                  sc.CreatedAt <= endDate)
            .GroupBy(c => new { c.CreatedAt.Year, c.CreatedAt.Month })
            .Select(g => new { Key = $"{g.Key.Year}-{g.Key.Month:D2}", Value = g.Sum(c => c.NetAmount) })
            .ToDictionaryAsync(x => x.Key, x => x.Value, cancellationToken);

        // Payouts by month
        var payoutsByMonth = await context.Set<CommissionPayout>()
            .AsNoTracking()
            .Where(p => p.SellerId == request.SellerId &&
                  p.CreatedAt >= startDate &&
                  p.CreatedAt <= endDate &&
                  p.Status == PayoutStatus.Completed)
            .GroupBy(p => new { p.CreatedAt.Year, p.CreatedAt.Month })
            .Select(g => new { Key = $"{g.Key.Year}-{g.Key.Month:D2}", Value = g.Sum(p => p.NetAmount) })
            .ToDictionaryAsync(x => x.Key, x => x.Value, cancellationToken);

        return new SellerFinanceSummaryDto
        {
            SellerId = request.SellerId,
            Balance = balance,
            RecentTransactions = transactions.Items.ToList(),
            RecentInvoices = invoices.Items.ToList(),
            EarningsByMonth = earningsByMonth,
            PayoutsByMonth = payoutsByMonth
        };
    }
}
