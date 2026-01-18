using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.DTOs.Order;
using Merge.Application.Interfaces;
using Merge.Domain.Enums;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Order.Queries.GetOrderStatistics;

public class GetOrderStatisticsQueryHandler(IDbContext context) : IRequestHandler<GetOrderStatisticsQuery, OrderStatisticsDto>
{

    public async Task<OrderStatisticsDto> Handle(GetOrderStatisticsQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-12);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        var query = context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.UserId == request.UserId &&
                  o.CreatedAt >= startDate &&
                  o.CreatedAt <= endDate);

        var totalOrders = await query.CountAsync(cancellationToken);
        var totalRevenue = await query
            .Where(o => o.PaymentStatus == PaymentStatus.Completed)
            .SumAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0;
        var pendingOrders = await query.CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);
        var completedOrders = await query.CountAsync(o => o.Status == OrderStatus.Delivered, cancellationToken);
        var cancelledOrders = await query.CountAsync(o => o.Status == OrderStatus.Cancelled, cancellationToken);

        var stats = new OrderStatisticsDto
        {
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            PendingOrders = pendingOrders,
            CompletedOrders = completedOrders,
            CancelledOrders = cancelledOrders
        };

        stats.AverageOrderValue = stats.TotalOrders > 0 
            ? stats.TotalRevenue / stats.TotalOrders 
            : 0;

        stats.OrdersByStatus = await query
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        stats.RevenueByMonth = await query
            .Where(o => o.PaymentStatus == PaymentStatus.Completed)
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new { 
                Key = $"{g.Key.Year}-{g.Key.Month:D2}", 
                Revenue = g.Sum(o => o.TotalAmount) 
            })
            .ToDictionaryAsync(x => x.Key, x => x.Revenue, cancellationToken);

        return stats;
    }
}
