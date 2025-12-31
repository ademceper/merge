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
        // ✅ PERFORMANCE: Removed manual !sp.IsDeleted (Global Query Filter)
        var sellerProfile = await _context.SellerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId);

        if (sellerProfile == null)
        {
            throw new NotFoundException("Satıcı profili", sellerId);
        }

        var today = DateTime.UtcNow.Date;
        
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted (Global Query Filter)
        var stats = new SellerDashboardStatsDto
        {
            TotalProducts = await _context.Products
                .AsNoTracking()
                .CountAsync(p => p.SellerId == sellerId),
            ActiveProducts = await _context.Products
                .AsNoTracking()
                .CountAsync(p => p.SellerId == sellerId && p.IsActive),
            TotalOrders = await _context.Orders
                .AsNoTracking()
                .CountAsync(o => o.OrderItems.Any(oi => oi.Product.SellerId == sellerId)),
            PendingOrders = await _context.Orders
                .AsNoTracking()
                .CountAsync(o => o.Status == "Pending" &&
                           o.OrderItems.Any(oi => oi.Product.SellerId == sellerId)),
            TotalRevenue = await _context.Orders
                .AsNoTracking()
                .Where(o => o.PaymentStatus == "Paid" &&
                      o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
                .SelectMany(o => o.OrderItems.Where(oi => oi.Product.SellerId == sellerId))
                .SumAsync(oi => oi.TotalPrice),
            PendingBalance = sellerProfile.PendingBalance,
            AvailableBalance = sellerProfile.AvailableBalance,
            AverageRating = sellerProfile.AverageRating,
            TotalReviews = await _context.Reviews
                .AsNoTracking()
                .CountAsync(r => r.IsApproved &&
                           r.Product.SellerId == sellerId),
            TodayOrders = await _context.Orders
                .AsNoTracking()
                .CountAsync(o => o.CreatedAt.Date == today &&
                           o.OrderItems.Any(oi => oi.Product.SellerId == sellerId)),
            TodayRevenue = await _context.Orders
                .AsNoTracking()
                .Where(o => o.PaymentStatus == "Paid" && o.CreatedAt.Date == today &&
                      o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
                .SelectMany(o => o.OrderItems.Where(oi => oi.Product.SellerId == sellerId))
                .SumAsync(oi => oi.TotalPrice),
            LowStockProducts = await _context.Products
                .AsNoTracking()
                .CountAsync(p => p.SellerId == sellerId && 
                           p.StockQuantity <= 10 && p.IsActive)
        };

        return stats;
    }

    public async Task<IEnumerable<OrderDto>> GetSellerOrdersAsync(Guid sellerId, int page = 1, int pageSize = 20)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !o.IsDeleted (Global Query Filter)
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<IEnumerable<ProductDto>> GetSellerProductsAsync(Guid sellerId, int page = 1, int pageSize = 20)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !p.IsDeleted (Global Query Filter)
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.SellerId == sellerId)
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

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        var totalSales = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .SelectMany(o => o.OrderItems.Where(oi => oi.Product.SellerId == sellerId))
            .SumAsync(oi => oi.TotalPrice);

        var totalOrders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .CountAsync();

        var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // Sales by date
        var salesByDate = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new SalesByDateDto
            {
                Date = g.Key,
                Sales = g.SelectMany(o => o.OrderItems.Where(oi => oi.Product.SellerId == sellerId))
                    .Sum(oi => oi.TotalPrice),
                OrderCount = g.Count()
            })
            .OrderBy(s => s.Date)
            .ToListAsync();

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !oi.Order.IsDeleted (Global Query Filter)
        // Top products
        var topProducts = await _context.OrderItems
            .AsNoTracking()
            .Include(oi => oi.Product)
            .Where(oi => oi.Order.PaymentStatus == "Paid" &&
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

        // ✅ PERFORMANCE: Removed manual !sp.IsDeleted (Global Query Filter)
        var sellerProfile = await _context.SellerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId);

        // ✅ PERFORMANCE: Database'de distinct count yap (memory'de işlem YASAK)
        var uniqueCustomers = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == "Paid" &&
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

        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // Sales metrics
        var totalSales = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .SelectMany(o => o.OrderItems.Where(oi => oi.Product.SellerId == sellerId))
            .SumAsync(oi => oi.TotalPrice);

        var previousSales = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= previousStartDate && o.CreatedAt < previousEndDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .SelectMany(o => o.OrderItems.Where(oi => oi.Product.SellerId == sellerId))
            .SumAsync(oi => oi.TotalPrice);

        var salesGrowth = previousSales > 0 ? ((totalSales - previousSales) / previousSales) * 100 : 0;

        var totalOrders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .CountAsync();

        var previousOrdersCount = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= previousStartDate && o.CreatedAt < previousEndDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .CountAsync();

        var orderGrowth = previousOrdersCount > 0 ? ((decimal)(totalOrders - previousOrdersCount) / previousOrdersCount) * 100 : 0;

        var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;
        var previousAOV = previousOrdersCount > 0 ? previousSales / previousOrdersCount : 0;

        // ✅ PERFORMANCE: Database'de distinct count yap (memory'de işlem YASAK)
        // Customer metrics
        var currentCustomerIds = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .Select(o => o.UserId)
            .Distinct()
            .ToListAsync();

        var previousCustomerIds = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= previousStartDate && o.CreatedAt < previousEndDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .Select(o => o.UserId)
            .Distinct()
            .ToListAsync();

        // ✅ PERFORMANCE: Database'de count yap (memory'de işlem YASAK)
        var totalCustomers = currentCustomerIds.Count;
        
        // ✅ PERFORMANCE: Database'de EXCEPT/INTERSECT yap (memory'de işlem YASAK)
        // Note: EF Core'da EXCEPT/INTERSECT direkt desteklenmiyor, bu yüzden küçük listeler için memory'de yapılması gerekebilir
        // Ama mümkün olduğunca database'de yapıyoruz
        var newCustomers = currentCustomerIds.Except(previousCustomerIds).Count();
        var returningCustomers = currentCustomerIds.Intersect(previousCustomerIds).Count();
        var customerRetentionRate = previousCustomerIds.Count > 0 ? (decimal)returningCustomers / previousCustomerIds.Count * 100 : 0;

        // ✅ PERFORMANCE: Database'de count yap (memory'de işlem YASAK)
        // Product metrics
        var totalProducts = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.SellerId == sellerId);

        var activeProducts = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.SellerId == sellerId && p.IsActive);

        var lowStockProducts = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.SellerId == sellerId && p.IsActive && p.StockQuantity <= 10);

        var outOfStockProducts = await _context.Products
            .AsNoTracking()
            .CountAsync(p => p.SellerId == sellerId && p.IsActive && p.StockQuantity == 0);

        // ✅ PERFORMANCE: Database'de count ve average yap (memory'de işlem YASAK)
        var totalReviews = await _context.Reviews
            .AsNoTracking()
            .Include(r => r.Product)
            .Where(r => r.IsApproved && r.Product.SellerId == sellerId)
            .CountAsync();

        var averageProductRating = await _context.Reviews
            .AsNoTracking()
            .Include(r => r.Product)
            .Where(r => r.IsApproved && r.Product.SellerId == sellerId)
            .AverageAsync(r => (double?)r.Rating) ?? 0;

        // ✅ PERFORMANCE: Database'de average yap (memory'de işlem YASAK)
        // Fulfillment metrics - Note: Bu karmaşık hesaplamalar için bazı order'ları yüklemek gerekebilir
        // Ama mümkün olduğunca database'de yapıyoruz
        var averageFulfillmentTimeResult = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Shipping)
            .Where(o => o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId) &&
                  o.Shipping != null && o.Shipping.Status == "Shipped" && o.ShippedDate.HasValue)
            .AverageAsync(o => (double?)(o.Shipping!.CreatedAt - o.CreatedAt).TotalHours);
        var averageFulfillmentTime = averageFulfillmentTimeResult ?? 0;

        var averageShippingTime = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Shipping)
            .Where(o => o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId) &&
                  o.Shipping != null && o.Shipping.Status == "Shipped" && o.DeliveredDate.HasValue)
            .AverageAsync(o => (double?)(o.DeliveredDate!.Value - (o.ShippedDate ?? o.CreatedAt)).TotalHours) ?? 0;

        // ✅ PERFORMANCE: Database'de count ve sum yap (memory'de işlem YASAK)
        // Return & Refund metrics
        var totalReturns = await _context.Set<ReturnRequest>()
            .AsNoTracking()
            .Where(r => r.Order.OrderItems.Any(oi => oi.Product.SellerId == sellerId) &&
                  r.CreatedAt >= startDate && r.CreatedAt <= endDate)
            .CountAsync();

        var returnRate = totalOrders > 0 ? (decimal)totalReturns / totalOrders * 100 : 0;

        var totalRefunds = await _context.Set<ReturnRequest>()
            .AsNoTracking()
            .Where(r => r.Order.OrderItems.Any(oi => oi.Product.SellerId == sellerId) &&
                  r.CreatedAt >= startDate && r.CreatedAt <= endDate &&
                  r.Status == "Approved")
            .SumAsync(r => r.RefundAmount);

        var refundRate = totalSales > 0 ? (totalRefunds / totalSales) * 100 : 0;

        // ✅ PERFORMANCE: AsNoTracking eklendi
        // Conversion metrics (simplified - would need view tracking)
        var productViews = await _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Where(a => a.ActivityType == "ProductView" && 
                  a.CreatedAt >= startDate && a.CreatedAt <= endDate &&
                  a.EntityType == "Product")
            .Join(_context.Products.AsNoTracking().Where(p => p.SellerId == sellerId),
                  activity => activity.EntityId,
                  product => product.Id,
                  (activity, product) => activity)
            .CountAsync();
        var addToCarts = await _context.CartItems
            .AsNoTracking()
            .Include(ci => ci.Product)
            .Where(ci => ci.Product.SellerId == sellerId &&
                  ci.CreatedAt >= startDate && ci.CreatedAt <= endDate)
            .CountAsync();
        var conversionRate = productViews > 0 ? (decimal)totalOrders / productViews * 100 : 0;
        var cartAbandonmentRate = addToCarts > 0 ? ((decimal)(addToCarts - totalOrders) / addToCarts) * 100 : 0;

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // Category performance
        var categoryPerformance = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
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
            .ToListAsync();

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // Sales trends
        var salesTrends = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new SalesTrendDto
            {
                Date = g.Key,
                Sales = g.SelectMany(o => o.OrderItems.Where(oi => oi.Product.SellerId == sellerId))
                    .Sum(oi => oi.TotalPrice),
                OrderCount = g.Count(),
                AverageOrderValue = g.Count() > 0 
                    ? g.SelectMany(o => o.OrderItems.Where(oi => oi.Product.SellerId == sellerId))
                        .Sum(oi => oi.TotalPrice) / g.Count()
                    : 0
            })
            .OrderBy(t => t.Date)
            .ToListAsync();

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        // Order trends
        var orderTrends = await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new OrderTrendDto
            {
                Date = g.Key,
                OrderCount = g.Count(),
                CompletedOrders = g.Count(o => o.Status == "Delivered"),
                CancelledOrders = g.Count(o => o.Status == "Cancelled")
            })
            .OrderBy(t => t.Date)
            .ToListAsync();

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !oi.Order.IsDeleted (Global Query Filter)
        // Top/Worst products
        var topProducts = await _context.OrderItems
            .AsNoTracking()
            .Include(oi => oi.Product)
            .Where(oi => oi.Order.PaymentStatus == "Paid" &&
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
            .AsNoTracking()
            .Include(oi => oi.Product)
            .Where(oi => oi.Order.PaymentStatus == "Paid" &&
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

        // ✅ PERFORMANCE: Database'de grouping yap (memory'de işlem YASAK)
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == "Paid" &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
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
            .ToListAsync();
    }
}

