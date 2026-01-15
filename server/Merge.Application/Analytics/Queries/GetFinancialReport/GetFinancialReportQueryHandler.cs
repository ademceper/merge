using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Configuration;
using Merge.Domain.Enums;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using AutoMapper;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Marketplace;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Analytics.Queries.GetFinancialReport;

public class GetFinancialReportQueryHandler(
    IDbContext context,
    ILogger<GetFinancialReportQueryHandler> logger,
    IOptions<AnalyticsSettings> settings,
    IMapper mapper) : IRequestHandler<GetFinancialReportQuery, FinancialReportDto>
{

    public async Task<FinancialReportDto> Handle(GetFinancialReportQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching financial report. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        var ordersQuery = context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= request.StartDate &&
                  o.CreatedAt <= request.EndDate);

        var totalRevenue = await ordersQuery.SumAsync(o => o.TotalAmount, cancellationToken);
        var productRevenue = await ordersQuery.SumAsync(o => o.SubTotal, cancellationToken);
        var shippingRevenue = await ordersQuery.SumAsync(o => o.ShippingCost, cancellationToken);
        var taxCollected = await ordersQuery.SumAsync(o => o.Tax, cancellationToken);
        var totalOrdersCount = await ordersQuery.CountAsync(cancellationToken);
        // ✅ PERFORMANCE: Subquery yaklaşımı - memory'de hiçbir şey tutma (ISSUE #3.1 fix)
        var orderIdsSubquery = from o in ordersQuery select o.Id;
        var productCosts = await context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => orderIdsSubquery.Contains(oi.OrderId))
            .SumAsync(oi => oi.UnitPrice * oi.Quantity * settings.Value.ProductCostPercentage, cancellationToken);
        
        var shippingCosts = await ordersQuery.SumAsync(o => o.ShippingCost * settings.Value.ShippingCostPercentage, cancellationToken);
        var platformFees = await ordersQuery.SumAsync(o => o.TotalAmount * settings.Value.PlatformFeePercentage, cancellationToken);
        var discountGiven = await ordersQuery.SumAsync(o => (o.CouponDiscount ?? 0) + (o.GiftCardDiscount ?? 0), cancellationToken);
        var commissionPaid = await context.Set<SellerCommission>()
            .AsNoTracking()
            .Where(sc => sc.CreatedAt >= request.StartDate &&
                  sc.CreatedAt <= request.EndDate)
            .SumAsync(sc => sc.CommissionAmount, cancellationToken);
        var refundAmount = await context.Set<ReturnRequest>()
            .AsNoTracking()
            .Where(r => r.Status == ReturnRequestStatus.Approved &&
                  r.CreatedAt >= request.StartDate &&
                  r.CreatedAt <= request.EndDate)
            .SumAsync(r => r.RefundAmount, cancellationToken);

        var totalCosts = productCosts + shippingCosts + platformFees + commissionPaid + refundAmount;
        var grossProfit = totalRevenue - productCosts - shippingCosts;
        var netProfit = totalRevenue - totalCosts;
        var profitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0;
        var periodDays = (request.EndDate - request.StartDate).Days;
        var previousStartDate = request.StartDate.AddDays(-periodDays);
        var previousEndDate = request.StartDate;
        var previousOrdersQuery = context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= previousStartDate &&
                  o.CreatedAt < previousEndDate);

        var previousRevenue = await previousOrdersQuery.SumAsync(o => o.TotalAmount, cancellationToken);
        var previousProfit = previousRevenue - (previousRevenue * settings.Value.DefaultCostPercentage);
        var revenueGrowth = previousRevenue > 0 ? ((totalRevenue - previousRevenue) / previousRevenue) * 100 : 0;
        var profitGrowth = previousProfit > 0 ? ((netProfit - previousProfit) / previousProfit) * 100 : 0;

        var revenueByCategory = await context.Set<OrderItem>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(oi => oi.Product)
            .ThenInclude(p => p.Category)
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.PaymentStatus == PaymentStatus.Completed &&
                  oi.Order.CreatedAt >= request.StartDate && 
                  oi.Order.CreatedAt <= request.EndDate)
            .GroupBy(oi => new { oi.Product.CategoryId, CategoryName = oi.Product.Category!.Name })
            .Select(g => new RevenueByCategoryDto(
                g.Key.CategoryId,
                g.Key.CategoryName,
                g.Sum(oi => oi.TotalPrice),
                g.Select(oi => oi.OrderId).Distinct().Count(),
                totalRevenue > 0 ? (g.Sum(oi => oi.TotalPrice) / totalRevenue) * 100 : 0
            ))
            .OrderByDescending(c => c.Revenue)
            .ToListAsync(cancellationToken);

        var revenueByDate = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= request.StartDate &&
                  o.CreatedAt <= request.EndDate)
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new RevenueByDateDto(
                g.Key,
                g.Sum(o => o.TotalAmount),
                g.Sum(o => o.TotalAmount * settings.Value.DefaultCostPercentage),
                g.Sum(o => o.TotalAmount * settings.Value.DefaultProfitPercentage),
                g.Count()
            ))
            .OrderBy(r => r.Date)
            .ToListAsync(cancellationToken);

        var expensesByTypeData = new[]
        {
            new { ExpenseType = "Product Costs", Amount = Math.Round(productCosts, 2), Percentage = totalCosts > 0 ? Math.Round((productCosts / totalCosts) * 100, 2) : 0 },
            new { ExpenseType = "Shipping Costs", Amount = Math.Round(shippingCosts, 2), Percentage = totalCosts > 0 ? Math.Round((shippingCosts / totalCosts) * 100, 2) : 0 },
            new { ExpenseType = "Commission Paid", Amount = Math.Round(commissionPaid, 2), Percentage = totalCosts > 0 ? Math.Round((commissionPaid / totalCosts) * 100, 2) : 0 },
            new { ExpenseType = "Refunds", Amount = Math.Round(refundAmount, 2), Percentage = totalCosts > 0 ? Math.Round((refundAmount / totalCosts) * 100, 2) : 0 },
            new { ExpenseType = "Discounts", Amount = Math.Round(discountGiven, 2), Percentage = totalCosts > 0 ? Math.Round((discountGiven / totalCosts) * 100, 2) : 0 },
            new { ExpenseType = "Platform Fees", Amount = Math.Round(platformFees, 2), Percentage = totalCosts > 0 ? Math.Round((platformFees / totalCosts) * 100, 2) : 0 }
        };

        var expensesByType = mapper.Map<List<ExpenseByTypeDto>>(expensesByTypeData);

        var financialReportData = new
        {
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalRevenue = Math.Round(totalRevenue, 2),
            ProductRevenue = Math.Round(productRevenue, 2),
            ShippingRevenue = Math.Round(shippingRevenue, 2),
            TaxCollected = Math.Round(taxCollected, 2),
            TotalCosts = Math.Round(totalCosts, 2),
            ProductCosts = Math.Round(productCosts, 2),
            ShippingCosts = Math.Round(shippingCosts, 2),
            PlatformFees = Math.Round(platformFees, 2),
            CommissionPaid = Math.Round(commissionPaid, 2),
            RefundAmount = Math.Round(refundAmount, 2),
            DiscountGiven = Math.Round(discountGiven, 2),
            GrossProfit = Math.Round(grossProfit, 2),
            NetProfit = Math.Round(netProfit, 2),
            ProfitMargin = Math.Round(profitMargin, 2),
            RevenueByCategory = revenueByCategory,
            RevenueByDate = revenueByDate,
            ExpensesByType = expensesByType,
            RevenueGrowth = Math.Round(revenueGrowth, 2),
            ProfitGrowth = Math.Round(profitGrowth, 2),
            AverageOrderValue = totalOrdersCount > 0 ? Math.Round(totalRevenue / totalOrdersCount, 2) : 0,
            TotalOrders = totalOrdersCount
        };

        return mapper.Map<FinancialReportDto>(financialReportData);
    }
}

