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
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetFinancialReport;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
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

        // Calculate revenue - Database'de aggregate
        var totalRevenue = await ordersQuery.SumAsync(o => o.TotalAmount, cancellationToken);
        var productRevenue = await ordersQuery.SumAsync(o => o.SubTotal, cancellationToken);
        var shippingRevenue = await ordersQuery.SumAsync(o => o.ShippingCost, cancellationToken);
        var taxCollected = await ordersQuery.SumAsync(o => o.Tax, cancellationToken);
        var totalOrdersCount = await ordersQuery.CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de OrderItems sum hesapla (memory'de Sum YASAK)
        // OrderItems'ı direkt database'den hesapla - orderIds ile join yap
        // ✅ PERFORMANCE: Batch loading pattern - orderIds için ToListAsync gerekli (Contains() için)
        var orderIds = await ordersQuery.Select(o => o.Id).ToListAsync(cancellationToken);
        // ✅ PERFORMANCE: orderIds boşsa Contains() hiçbir şey döndürmez, direkt SumAsync çağırabiliriz
        // List.Count kontrolü gerekmez çünkü boş liste Contains() ile hiçbir şey match etmez
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var productCosts = await context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => orderIds.Contains(oi.OrderId))
            .SumAsync(oi => oi.UnitPrice * oi.Quantity * settings.Value.ProductCostPercentage, cancellationToken);
        
        // ✅ PERFORMANCE: Basit aggregateler database'de yapılabilir ama orders zaten çekilmiş (OrderItems için)
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var shippingCosts = await ordersQuery.SumAsync(o => o.ShippingCost * settings.Value.ShippingCostPercentage, cancellationToken);
        var platformFees = await ordersQuery.SumAsync(o => o.TotalAmount * settings.Value.PlatformFeePercentage, cancellationToken);
        var discountGiven = await ordersQuery.SumAsync(o => (o.CouponDiscount ?? 0) + (o.GiftCardDiscount ?? 0), cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !sc.IsDeleted and !r.IsDeleted checks (Global Query Filter handles it)
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

        // Previous period for comparison
        var periodDays = (request.EndDate - request.StartDate).Days;
        var previousStartDate = request.StartDate.AddDays(-periodDays);
        var previousEndDate = request.StartDate;

        // ✅ PERFORMANCE: Database'de toplam hesapla (memory'de Sum YASAK)
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted check (Global Query Filter handles it)
        var previousOrdersQuery = context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= previousStartDate &&
                  o.CreatedAt < previousEndDate);

        var previousRevenue = await previousOrdersQuery.SumAsync(o => o.TotalAmount, cancellationToken);
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var previousProfit = previousRevenue - (previousRevenue * settings.Value.DefaultCostPercentage);
        var revenueGrowth = previousRevenue > 0 ? ((totalRevenue - previousRevenue) / previousRevenue) * 100 : 0;
        var profitGrowth = previousProfit > 0 ? ((netProfit - previousProfit) / previousProfit) * 100 : 0;

        // ✅ PERFORMANCE: Revenue by category - Database'de grouping yap (memory'de değil)
        var revenueByCategory = await context.Set<OrderItem>()
            .AsNoTracking()
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

        // ✅ PERFORMANCE: Revenue by date - Database'de grouping yap (memory'de değil)
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

        // Expenses by type - Database'de oluşturulamaz, hesaplanmış değerler
        // ✅ ARCHITECTURE: .cursorrules'a göre manuel mapping YASAK, AutoMapper kullanıyoruz
        var expensesByTypeData = new[]
        {
            new { ExpenseType = "Product Costs", Amount = Math.Round(productCosts, 2), Percentage = totalCosts > 0 ? Math.Round((productCosts / totalCosts) * 100, 2) : 0 },
            new { ExpenseType = "Shipping Costs", Amount = Math.Round(shippingCosts, 2), Percentage = totalCosts > 0 ? Math.Round((shippingCosts / totalCosts) * 100, 2) : 0 },
            new { ExpenseType = "Commission Paid", Amount = Math.Round(commissionPaid, 2), Percentage = totalCosts > 0 ? Math.Round((commissionPaid / totalCosts) * 100, 2) : 0 },
            new { ExpenseType = "Refunds", Amount = Math.Round(refundAmount, 2), Percentage = totalCosts > 0 ? Math.Round((refundAmount / totalCosts) * 100, 2) : 0 },
            new { ExpenseType = "Discounts", Amount = Math.Round(discountGiven, 2), Percentage = totalCosts > 0 ? Math.Round((discountGiven / totalCosts) * 100, 2) : 0 },
            new { ExpenseType = "Platform Fees", Amount = Math.Round(platformFees, 2), Percentage = totalCosts > 0 ? Math.Round((platformFees / totalCosts) * 100, 2) : 0 }
        };

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var expensesByType = mapper.Map<List<ExpenseByTypeDto>>(expensesByTypeData);

        // ✅ ARCHITECTURE: FinancialReportDto entity'den gelmiyor, hesaplanmış değerler olduğu için
        // anonymous type'dan DTO'ya AutoMapper ile mapping yapıyoruz (property isimleri aynı olduğu için otomatik map eder)
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

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return mapper.Map<FinancialReportDto>(financialReportData);
    }
}

