using AutoMapper;
using OrderEntity = Merge.Domain.Entities.Order;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Order;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Order;


namespace Merge.Application.Services.Order;

public class OrderFilterService : IOrderFilterService
{
    private readonly IRepository<OrderEntity> _orderRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public OrderFilterService(
        IRepository<OrderEntity> orderRepository,
        ApplicationDbContext context,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrderDto>> GetFilteredOrdersAsync(OrderFilterDto filter)
    {
        // ✅ PERFORMANCE: AsNoTracking + Global Query Filter automatically filters !o.IsDeleted
        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Address)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .AsQueryable();

        if (filter.UserId.HasValue)
        {
            query = query.Where(o => o.UserId == filter.UserId.Value);
        }

        if (!string.IsNullOrEmpty(filter.Status))
        {
            query = query.Where(o => o.Status == filter.Status);
        }

        if (!string.IsNullOrEmpty(filter.PaymentStatus))
        {
            query = query.Where(o => o.PaymentStatus == filter.PaymentStatus);
        }

        if (filter.StartDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= filter.EndDate.Value);
        }

        if (filter.MinAmount.HasValue)
        {
            query = query.Where(o => o.TotalAmount >= filter.MinAmount.Value);
        }

        if (filter.MaxAmount.HasValue)
        {
            query = query.Where(o => o.TotalAmount <= filter.MaxAmount.Value);
        }

        if (!string.IsNullOrEmpty(filter.OrderNumber))
        {
            // ✅ PERFORMANCE: Use EF.Functions.ILike for case-insensitive search
            query = query.Where(o => EF.Functions.ILike(o.OrderNumber, $"%{filter.OrderNumber}%"));
        }

        // Sıralama
        query = filter.SortBy?.ToLower() switch
        {
            "amount" => filter.SortDescending 
                ? query.OrderByDescending(o => o.TotalAmount)
                : query.OrderBy(o => o.TotalAmount),
            "status" => filter.SortDescending
                ? query.OrderByDescending(o => o.Status)
                : query.OrderBy(o => o.Status),
            _ => filter.SortDescending
                ? query.OrderByDescending(o => o.CreatedAt)
                : query.OrderBy(o => o.CreatedAt)
        };

        // Sayfalama
        var orders = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderStatisticsDto> GetOrderStatisticsAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.AddMonths(-12);
        endDate ??= DateTime.UtcNow;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted (Global Query Filter)
        var query = _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId &&
                  o.CreatedAt >= startDate.Value &&
                  o.CreatedAt <= endDate.Value);

        // ✅ PERFORMANCE: Database'de aggregation yap (ToListAsync() sonrası işlem YASAK)
        var totalOrders = await query.CountAsync();
        var totalRevenue = await query
            .Where(o => o.PaymentStatus == "Paid")
            .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;
        var pendingOrders = await query.CountAsync(o => o.Status == "Pending");
        var completedOrders = await query.CountAsync(o => o.Status == "Delivered");
        var cancelledOrders = await query.CountAsync(o => o.Status == "Cancelled");

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

        // ✅ PERFORMANCE: Database'de grouping yap (ToListAsync() sonrası GroupBy YASAK)
        var ordersByStatus = await query
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();
        
        stats.OrdersByStatus = ordersByStatus.ToDictionary(x => x.Status, x => x.Count);

        // ✅ PERFORMANCE: Database'de grouping yap (ToListAsync() sonrası GroupBy YASAK)
        var revenueByMonth = await query
            .Where(o => o.PaymentStatus == "Paid")
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .Select(g => new { 
                Key = $"{g.Key.Year}-{g.Key.Month:D2}", 
                Revenue = g.Sum(o => o.TotalAmount) 
            })
            .ToListAsync();
        
        stats.RevenueByMonth = revenueByMonth.ToDictionary(x => x.Key, x => x.Revenue);

        return stats;
    }
}

