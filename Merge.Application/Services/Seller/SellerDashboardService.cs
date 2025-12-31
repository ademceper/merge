using AutoMapper;
using UserEntity = Merge.Domain.Entities.User;
using OrderEntity = Merge.Domain.Entities.Order;
using ProductEntity = Merge.Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Seller;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Merge.Application.DTOs.Order;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Seller;


namespace Merge.Application.Services.Seller;

public class SellerDashboardService : ISellerDashboardService
{
    private readonly IRepository<SellerProfile> _sellerProfileRepository;
    private readonly IRepository<ProductEntity> _productRepository;
    private readonly IRepository<OrderEntity> _orderRepository;
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SellerDashboardService(
        IRepository<SellerProfile> sellerProfileRepository,
        IRepository<ProductEntity> productRepository,
        IRepository<OrderEntity> orderRepository,
        ApplicationDbContext context,
        IMapper mapper)
    {
        _sellerProfileRepository = sellerProfileRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _context = context;
        _mapper = mapper;
    }

    public async Task<SellerDashboardStatsDto> GetDashboardStatsAsync(Guid sellerId)
    {
        var sellerProfile = await _context.SellerProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId && !sp.IsDeleted);

        if (sellerProfile == null)
        {
            throw new NotFoundException("Satıcı profili", sellerId);
        }

        var today = DateTime.UtcNow.Date;
        var stats = new SellerDashboardStatsDto
        {
            TotalProducts = await _context.Products
                .CountAsync(p => p.SellerId == sellerId && !p.IsDeleted),
            ActiveProducts = await _context.Products
                .CountAsync(p => p.SellerId == sellerId && !p.IsDeleted && p.IsActive),
            TotalOrders = await _context.Orders
                .CountAsync(o => !o.IsDeleted && 
                           o.OrderItems.Any(oi => oi.Product.SellerId == sellerId)),
            PendingOrders = await _context.Orders
                .CountAsync(o => !o.IsDeleted && o.Status == "Pending" &&
                           o.OrderItems.Any(oi => oi.Product.SellerId == sellerId)),
            TotalRevenue = await _context.Orders
                .Where(o => !o.IsDeleted && o.PaymentStatus == "Paid" &&
                      o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
                .SumAsync(o => o.OrderItems
                    .Where(oi => oi.Product.SellerId == sellerId)
                    .Sum(oi => oi.TotalPrice)),
            PendingBalance = sellerProfile.PendingBalance,
            AvailableBalance = sellerProfile.AvailableBalance,
            AverageRating = sellerProfile.AverageRating,
            TotalReviews = await _context.Reviews
                .CountAsync(r => !r.IsDeleted && r.IsApproved &&
                           r.Product.SellerId == sellerId),
            TodayOrders = await _context.Orders
                .CountAsync(o => !o.IsDeleted && o.CreatedAt.Date == today &&
                           o.OrderItems.Any(oi => oi.Product.SellerId == sellerId)),
            TodayRevenue = await _context.Orders
                .Where(o => !o.IsDeleted && o.PaymentStatus == "Paid" && o.CreatedAt.Date == today &&
                      o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
                .SumAsync(o => o.OrderItems
                    .Where(oi => oi.Product.SellerId == sellerId)
                    .Sum(oi => oi.TotalPrice)),
            LowStockProducts = await _context.Products
                .CountAsync(p => p.SellerId == sellerId && !p.IsDeleted && 
                           p.StockQuantity <= 10 && p.IsActive)
        };

        return stats;
    }

    public async Task<IEnumerable<OrderDto>> GetSellerOrdersAsync(Guid sellerId, int page = 1, int pageSize = 20)
    {
        var orders = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => !o.IsDeleted &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<IEnumerable<ProductDto>> GetSellerProductsAsync(Guid sellerId, int page = 1, int pageSize = 20)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => p.SellerId == sellerId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<SellerPerformanceDto> GetPerformanceMetricsAsync(Guid sellerId, DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => !o.IsDeleted && o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .ToListAsync();

        var sellerOrders = orders
            .Where(o => o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .ToList();

        var totalSales = sellerOrders
            .Sum(o => o.OrderItems
                .Where(oi => oi.Product.SellerId == sellerId)
                .Sum(oi => oi.TotalPrice));

        var totalOrders = sellerOrders.Count;
        var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

        // Sales by date
        var salesByDate = sellerOrders
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new SalesByDateDto
            {
                Date = g.Key,
                Sales = g.Sum(o => o.OrderItems
                    .Where(oi => oi.Product.SellerId == sellerId)
                    .Sum(oi => oi.TotalPrice)),
                OrderCount = g.Count()
            })
            .OrderBy(s => s.Date)
            .ToList();

        // Top products
        var topProducts = await _context.OrderItems
            .Include(oi => oi.Product)
            .Where(oi => !oi.Order.IsDeleted && oi.Order.PaymentStatus == "Paid" &&
                  oi.Order.CreatedAt >= startDate && oi.Order.CreatedAt <= endDate &&
                  oi.Product.SellerId == sellerId)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
            .Select(g => new SellerTopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                QuantitySold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.TotalPrice)
            })
            .OrderByDescending(p => p.Revenue)
            .Take(10)
            .ToListAsync();

        var sellerProfile = await _context.SellerProfiles
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId && !sp.IsDeleted);

        var uniqueCustomers = await _context.Orders
            .Where(o => !o.IsDeleted && o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .Select(o => o.UserId)
            .Distinct()
            .CountAsync();

        return new SellerPerformanceDto
        {
            TotalSales = totalSales,
            TotalOrders = totalOrders,
            AverageOrderValue = averageOrderValue,
            ConversionRate = 0, // Bu hesaplama için daha fazla veri gerekir
            AverageRating = sellerProfile?.AverageRating ?? 0,
            TotalCustomers = uniqueCustomers,
            SalesByDate = salesByDate,
            TopProducts = topProducts
        };
    }

    public async Task<SellerPerformanceMetricsDto> GetDetailedPerformanceMetricsAsync(Guid sellerId, DateTime startDate, DateTime endDate)
    {
        var periodDays = (endDate - startDate).Days;
        var previousStartDate = startDate.AddDays(-periodDays);
        var previousEndDate = startDate;

        // Current period data
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Category)
            .Include(o => o.Shipping)
            .Where(o => !o.IsDeleted && o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .ToListAsync();

        var sellerOrders = orders
            .Where(o => o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .ToList();

        // Previous period data
        var previousOrders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => !o.IsDeleted && o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= previousStartDate && o.CreatedAt < previousEndDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .ToListAsync();

        var previousSellerOrders = previousOrders
            .Where(o => o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .ToList();

        // Sales metrics
        var totalSales = sellerOrders.Sum(o => o.OrderItems
            .Where(oi => oi.Product.SellerId == sellerId)
            .Sum(oi => oi.TotalPrice));
        var previousSales = previousSellerOrders.Sum(o => o.OrderItems
            .Where(oi => oi.Product.SellerId == sellerId)
            .Sum(oi => oi.TotalPrice));
        var salesGrowth = previousSales > 0 ? ((totalSales - previousSales) / previousSales) * 100 : 0;

        var totalOrders = sellerOrders.Count;
        var previousOrdersCount = previousSellerOrders.Count;
        var orderGrowth = previousOrdersCount > 0 ? ((decimal)(totalOrders - previousOrdersCount) / previousOrdersCount) * 100 : 0;

        var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;
        var previousAOV = previousOrdersCount > 0 ? previousSales / previousOrdersCount : 0;

        // Customer metrics
        var customerIds = sellerOrders.Select(o => o.UserId).Distinct().ToList();
        var previousCustomerIds = previousSellerOrders.Select(o => o.UserId).Distinct().ToList();
        var totalCustomers = customerIds.Count;
        var newCustomers = customerIds.Except(previousCustomerIds).Count();
        var returningCustomers = customerIds.Intersect(previousCustomerIds).Count();
        var customerRetentionRate = previousCustomerIds.Any() ? (decimal)returningCustomers / previousCustomerIds.Count * 100 : 0;

        // Product metrics
        var products = await _context.Products
            .Where(p => p.SellerId == sellerId && !p.IsDeleted)
            .ToListAsync();
        var totalProducts = products.Count;
        var activeProducts = products.Count(p => p.IsActive);
        var lowStockProducts = products.Count(p => p.IsActive && p.StockQuantity <= 10);
        var outOfStockProducts = products.Count(p => p.IsActive && p.StockQuantity == 0);

        var reviews = await _context.Reviews
            .Include(r => r.Product)
            .Where(r => !r.IsDeleted && r.IsApproved && r.Product.SellerId == sellerId)
            .ToListAsync();
        var totalReviews = reviews.Count;
        var averageProductRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;

        // Fulfillment metrics
        var shippedOrders = sellerOrders.Where(o => o.Shipping != null && o.Shipping.Status == "Shipped").ToList();
        var averageFulfillmentTime = shippedOrders.Any() 
            ? shippedOrders.Average(o => o.Shipping != null && o.ShippedDate.HasValue 
                ? (o.Shipping.CreatedAt - o.CreatedAt).TotalHours 
                : 0) 
            : 0;
        var averageShippingTime = shippedOrders.Any() && shippedOrders.Any(o => o.DeliveredDate.HasValue)
            ? shippedOrders.Where(o => o.DeliveredDate.HasValue)
                .Average(o => (o.DeliveredDate!.Value - (o.ShippedDate ?? o.CreatedAt)).TotalHours)
            : 0;

        // Return & Refund metrics
        var returns = await _context.Set<ReturnRequest>()
            .Include(r => r.Order)
                .ThenInclude(o => o.OrderItems)
            .Where(r => !r.IsDeleted && r.Order.OrderItems.Any(oi => oi.Product.SellerId == sellerId) &&
                  r.CreatedAt >= startDate && r.CreatedAt <= endDate)
            .ToListAsync();
        var totalReturns = returns.Count;
        var returnRate = totalOrders > 0 ? (decimal)totalReturns / totalOrders * 100 : 0;
        var totalRefunds = returns.Where(r => r.Status == "Approved").Sum(r => r.RefundAmount);
        var refundRate = totalSales > 0 ? (totalRefunds / totalSales) * 100 : 0;

        // Conversion metrics (simplified - would need view tracking)
        var productViews = await _context.Set<UserActivityLog>()
            .Where(a => a.ActivityType == "ProductView" && 
                  a.CreatedAt >= startDate && a.CreatedAt <= endDate &&
                  a.EntityType == "Product")
            .Join(_context.Products.Where(p => p.SellerId == sellerId),
                  activity => activity.EntityId,
                  product => product.Id,
                  (activity, product) => activity)
            .CountAsync();
        var addToCarts = await _context.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.Product.SellerId == sellerId &&
                  ci.CreatedAt >= startDate && ci.CreatedAt <= endDate)
            .CountAsync();
        var conversionRate = productViews > 0 ? (decimal)totalOrders / productViews * 100 : 0;
        var cartAbandonmentRate = addToCarts > 0 ? ((decimal)(addToCarts - totalOrders) / addToCarts) * 100 : 0;

        // Category performance
        var categoryPerformance = sellerOrders
            .SelectMany(o => o.OrderItems.Where(oi => oi.Product.SellerId == sellerId))
            .GroupBy(oi => new { oi.Product.CategoryId, CategoryName = oi.Product.Category.Name })
            .Select(g => new CategoryPerformanceDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                ProductCount = g.Select(oi => oi.ProductId).Distinct().Count(),
                OrdersCount = g.Select(oi => oi.OrderId).Distinct().Count(),
                Revenue = g.Sum(oi => oi.TotalPrice),
                AverageRating = 0 // Would need to calculate from reviews
            })
            .OrderByDescending(c => c.Revenue)
            .ToList();

        // Sales trends
        var salesTrends = sellerOrders
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new SalesTrendDto
            {
                Date = g.Key,
                Sales = g.Sum(o => o.OrderItems.Where(oi => oi.Product.SellerId == sellerId).Sum(oi => oi.TotalPrice)),
                OrderCount = g.Count(),
                AverageOrderValue = g.Count() > 0 
                    ? g.Sum(o => o.OrderItems.Where(oi => oi.Product.SellerId == sellerId).Sum(oi => oi.TotalPrice)) / g.Count()
                    : 0
            })
            .OrderBy(t => t.Date)
            .ToList();

        // Order trends
        var orderTrends = sellerOrders
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new OrderTrendDto
            {
                Date = g.Key,
                OrderCount = g.Count(),
                CompletedOrders = g.Count(o => o.Status == "Delivered"),
                CancelledOrders = g.Count(o => o.Status == "Cancelled")
            })
            .OrderBy(t => t.Date)
            .ToList();

        // Top/Worst products
        var topProducts = await _context.OrderItems
            .Include(oi => oi.Product)
            .Where(oi => !oi.Order.IsDeleted && oi.Order.PaymentStatus == "Paid" &&
                  oi.Order.CreatedAt >= startDate && oi.Order.CreatedAt <= endDate &&
                  oi.Product.SellerId == sellerId)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
            .Select(g => new SellerTopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                QuantitySold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.TotalPrice)
            })
            .OrderByDescending(p => p.Revenue)
            .Take(10)
            .ToListAsync();

        var worstProducts = await _context.OrderItems
            .Include(oi => oi.Product)
            .Where(oi => !oi.Order.IsDeleted && oi.Order.PaymentStatus == "Paid" &&
                  oi.Order.CreatedAt >= startDate && oi.Order.CreatedAt <= endDate &&
                  oi.Product.SellerId == sellerId)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name })
            .Select(g => new SellerTopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                QuantitySold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.TotalPrice)
            })
            .OrderBy(p => p.Revenue)
            .Take(10)
            .ToListAsync();

        return new SellerPerformanceMetricsDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalSales = totalSales,
            PreviousPeriodSales = previousSales,
            SalesGrowth = Math.Round(salesGrowth, 2),
            TotalOrders = totalOrders,
            PreviousPeriodOrders = previousOrdersCount,
            OrderGrowth = Math.Round(orderGrowth, 2),
            AverageOrderValue = Math.Round(averageOrderValue, 2),
            PreviousPeriodAOV = Math.Round(previousAOV, 2),
            TotalCustomers = totalCustomers,
            NewCustomers = newCustomers,
            ReturningCustomers = returningCustomers,
            CustomerRetentionRate = Math.Round(customerRetentionRate, 2),
            CustomerLifetimeValue = totalCustomers > 0 ? Math.Round(totalSales / totalCustomers, 2) : 0,
            TotalProducts = totalProducts,
            ActiveProducts = activeProducts,
            LowStockProducts = lowStockProducts,
            OutOfStockProducts = outOfStockProducts,
            AverageProductRating = Math.Round((decimal)averageProductRating, 2),
            TotalReviews = totalReviews,
            AverageFulfillmentTime = Math.Round((decimal)averageFulfillmentTime, 2),
            AverageShippingTime = Math.Round((decimal)averageShippingTime, 2),
            OnTimeDeliveryRate = 0, // Would need delivery date tracking
            LateDeliveries = 0,
            TotalReturns = totalReturns,
            ReturnRate = Math.Round(returnRate, 2),
            TotalRefunds = totalRefunds,
            RefundRate = Math.Round(refundRate, 2),
            ProductViews = productViews,
            AddToCarts = addToCarts,
            ConversionRate = Math.Round(conversionRate, 2),
            CartAbandonmentRate = Math.Round(cartAbandonmentRate, 2),
            CategoryPerformance = categoryPerformance,
            SalesTrends = salesTrends,
            OrderTrends = orderTrends,
            TopProducts = topProducts,
            WorstProducts = worstProducts
        };
    }

    public async Task<List<CategoryPerformanceDto>> GetCategoryPerformanceAsync(Guid sellerId, DateTime? startDate = null, DateTime? endDate = null)
    {
        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                    .ThenInclude(p => p.Category)
            .Where(o => !o.IsDeleted && o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .ToListAsync();

        return orders
            .SelectMany(o => o.OrderItems.Where(oi => oi.Product.SellerId == sellerId))
            .GroupBy(oi => new { oi.Product.CategoryId, CategoryName = oi.Product.Category.Name })
            .Select(g => new CategoryPerformanceDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                ProductCount = g.Select(oi => oi.ProductId).Distinct().Count(),
                OrdersCount = g.Select(oi => oi.OrderId).Distinct().Count(),
                Revenue = g.Sum(oi => oi.TotalPrice),
                AverageRating = 0
            })
            .OrderByDescending(c => c.Revenue)
            .ToList();
    }
}

