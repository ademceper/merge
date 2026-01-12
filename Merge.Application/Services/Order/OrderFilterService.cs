using AutoMapper;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Order;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Application.DTOs.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.Order;

public class OrderFilterService : IOrderFilterService
{
    private readonly Merge.Application.Interfaces.IRepository<OrderEntity> _orderRepository;
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<OrderFilterService> _logger;

    public OrderFilterService(
        Merge.Application.Interfaces.IRepository<OrderEntity> orderRepository,
        IDbContext context,
        IMapper mapper,
        ILogger<OrderFilterService> logger)
    {
        _orderRepository = orderRepository;
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<OrderDto>> GetFilteredOrdersAsync(OrderFilterDto filter, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking + Global Query Filter automatically filters !o.IsDeleted
        var query = _context.Set<OrderEntity>()
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
            var statusEnum = Enum.Parse<OrderStatus>(filter.Status);
            query = query.Where(o => o.Status == statusEnum);
        }

        if (!string.IsNullOrEmpty(filter.PaymentStatus))
        {
            var paymentStatusEnum = Enum.Parse<PaymentStatus>(filter.PaymentStatus);
            query = query.Where(o => o.PaymentStatus == paymentStatusEnum);
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
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<OrderStatisticsDto> GetOrderStatisticsAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        startDate ??= DateTime.UtcNow.AddMonths(-12);
        endDate ??= DateTime.UtcNow;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted (Global Query Filter)
        var query = _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.UserId == userId &&
                  o.CreatedAt >= startDate.Value &&
                  o.CreatedAt <= endDate.Value);

        // ✅ PERFORMANCE: Database'de aggregation yap (ToListAsync() sonrası işlem YASAK)
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

        // ✅ PERFORMANCE: Database'de grouping ve ToDictionaryAsync yap (ToListAsync() sonrası ToDictionary YASAK)
        stats.OrdersByStatus = await query
            .GroupBy(o => o.Status)
            .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

        // ✅ PERFORMANCE: Database'de grouping ve ToDictionaryAsync yap (ToListAsync() sonrası ToDictionary YASAK)
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

