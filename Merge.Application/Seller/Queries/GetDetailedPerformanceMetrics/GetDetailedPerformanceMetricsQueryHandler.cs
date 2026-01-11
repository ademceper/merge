using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Seller;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using OrderEntity = Merge.Domain.Entities.Order;
using ProductEntity = Merge.Domain.Entities.Product;
using ReviewEntity = Merge.Domain.Entities.Review;

namespace Merge.Application.Seller.Queries.GetDetailedPerformanceMetrics;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetDetailedPerformanceMetricsQueryHandler : IRequestHandler<GetDetailedPerformanceMetricsQuery, SellerPerformanceMetricsDto>
{
    private readonly IDbContext _context;
    private readonly ILogger<GetDetailedPerformanceMetricsQueryHandler> _logger;

    public GetDetailedPerformanceMetricsQueryHandler(
        IDbContext context,
        ILogger<GetDetailedPerformanceMetricsQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SellerPerformanceMetricsDto> Handle(GetDetailedPerformanceMetricsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Getting detailed performance metrics. SellerId: {SellerId}, StartDate: {StartDate}, EndDate: {EndDate}",
            request.SellerId, request.StartDate, request.EndDate);

        var periodDays = (request.EndDate - request.StartDate).Days;
        var previousStartDate = request.StartDate.AddDays(-periodDays);
        var previousEndDate = request.StartDate;

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // Sales metrics
        var totalSales = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= request.StartDate && o.CreatedAt <= request.EndDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == request.SellerId))
            .SelectMany(o => o.OrderItems.Where(oi => oi.Product.SellerId == request.SellerId))
            .SumAsync(oi => oi.TotalPrice, cancellationToken);

        var previousSales = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= previousStartDate && o.CreatedAt < previousEndDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == request.SellerId))
            .SelectMany(o => o.OrderItems.Where(oi => oi.Product.SellerId == request.SellerId))
            .SumAsync(oi => oi.TotalPrice, cancellationToken);

        var salesGrowth = previousSales > 0 ? ((totalSales - previousSales) / previousSales) * 100 : 0;

        var totalOrders = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= request.StartDate && o.CreatedAt <= request.EndDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == request.SellerId))
            .CountAsync(cancellationToken);

        var previousOrders = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= previousStartDate && o.CreatedAt < previousEndDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == request.SellerId))
            .CountAsync(cancellationToken);

        var ordersGrowth = previousOrders > 0 ? ((totalOrders - previousOrders) * 100.0m / previousOrders) : 0;

        var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

        // ✅ PERFORMANCE: Database'de distinct count yap (memory'de işlem YASAK)
        var uniqueCustomers = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= request.StartDate && o.CreatedAt <= request.EndDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == request.SellerId))
            .Select(o => o.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de average yap (memory'de işlem YASAK)
        var averageRating = await _context.Set<ReviewEntity>()
            .AsNoTracking()
            .Where(r => r.IsApproved &&
                  r.Product.SellerId == request.SellerId &&
                  r.CreatedAt >= request.StartDate && r.CreatedAt <= request.EndDate)
            .AverageAsync(r => (double?)r.Rating, cancellationToken) ?? 0;

        return new SellerPerformanceMetricsDto
        {
            TotalSales = totalSales,
            SalesGrowth = salesGrowth,
            TotalOrders = totalOrders,
            OrdersGrowth = ordersGrowth,
            AverageOrderValue = averageOrderValue,
            UniqueCustomers = uniqueCustomers,
            AverageRating = (decimal)averageRating
        };
    }
}
