using AutoMapper;
using UserEntity = Merge.Domain.Modules.Identity.User;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Seller;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Domain.Enums;
using Merge.Application.DTOs.Order;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Seller;
using Merge.Application.Common;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketplace;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;
using ISellerProfileRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Marketplace.SellerProfile>;
using IProductRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Catalog.Product>;
using IOrderRepository = Merge.Application.Interfaces.IRepository<Merge.Domain.Modules.Ordering.Order>;

namespace Merge.Application.Services.Seller;

public class SellerDashboardService(ISellerProfileRepository sellerProfileRepository, IProductRepository productRepository, IOrderRepository orderRepository, IDbContext context, IMapper mapper, ILogger<SellerDashboardService> logger, IOptions<SellerSettings> sellerSettings, IOptions<PaginationSettings> paginationSettings) : ISellerDashboardService
{
    private readonly SellerSettings sellerConfig = sellerSettings.Value;
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;

    public async Task<SellerDashboardStatsDto> GetDashboardStatsAsync(Guid sellerId, CancellationToken cancellationToken = default)
    {
        var sellerProfile = await context.Set<SellerProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId, cancellationToken);

        if (sellerProfile == null)
        {
            throw new NotFoundException("Satıcı profili", sellerId);
        }

        var today = DateTime.UtcNow.Date;
        
        var stats = new SellerDashboardStatsDto
        {
            TotalProducts = await context.Set<ProductEntity>()
                .AsNoTracking()
                .CountAsync(p => p.SellerId == sellerId, cancellationToken),
            ActiveProducts = await context.Set<ProductEntity>()
                .AsNoTracking()
                .CountAsync(p => p.SellerId == sellerId && p.IsActive, cancellationToken),
            TotalOrders = await context.Set<OrderEntity>()
                .AsNoTracking()
                .CountAsync(o => o.OrderItems.Any(oi => oi.Product.SellerId == sellerId), cancellationToken),
            PendingOrders = await context.Set<OrderEntity>()
                .AsNoTracking()
                .CountAsync(o => o.Status == OrderStatus.Pending &&
                           o.OrderItems.Any(oi => oi.Product.SellerId == sellerId), cancellationToken),
            TotalRevenue = await (
                from o in context.Set<OrderEntity>().AsNoTracking()
                join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
                join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
                where o.PaymentStatus == PaymentStatus.Completed && p.SellerId == sellerId
                select oi.TotalPrice
            ).SumAsync(cancellationToken),
            PendingBalance = sellerProfile.PendingBalance,
            AvailableBalance = sellerProfile.AvailableBalance,
            AverageRating = sellerProfile.AverageRating,
            TotalReviews = await context.Set<ReviewEntity>()
                .AsNoTracking()
                .CountAsync(r => r.IsApproved &&
                           r.Product.SellerId == sellerId, cancellationToken),
            TodayOrders = await context.Set<OrderEntity>()
                .AsNoTracking()
                .CountAsync(o => o.CreatedAt.Date == today &&
                           o.OrderItems.Any(oi => oi.Product.SellerId == sellerId), cancellationToken),
            TodayRevenue = await (
                from o in context.Set<OrderEntity>().AsNoTracking()
                join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
                join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
                where o.PaymentStatus == PaymentStatus.Completed && 
                      o.CreatedAt.Date == today && 
                      p.SellerId == sellerId
                select oi.TotalPrice
            ).SumAsync(cancellationToken),
            LowStockProducts = await context.Set<ProductEntity>()
                .AsNoTracking()
                .CountAsync(p => p.SellerId == sellerId &&
                           p.StockQuantity <= sellerConfig.LowStockThreshold && p.IsActive, cancellationToken)
        };

        return stats;
    }

    public async Task<PagedResult<OrderDto>> GetSellerOrdersAsync(Guid sellerId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        if (page < 1) page = 1;

        IQueryable<OrderEntity> query = context.Set<OrderEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.OrderItems.Any(oi => oi.Product.SellerId == sellerId));

        var totalCount = await query.CountAsync(cancellationToken);

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var orderDtos = mapper.Map<IEnumerable<OrderDto>>(orders).ToList();

        return new PagedResult<OrderDto>
        {
            Items = orderDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<ProductDto>> GetSellerProductsAsync(Guid sellerId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageSize > paginationConfig.MaxPageSize) pageSize = paginationConfig.MaxPageSize;
        if (page < 1) page = 1;

        IQueryable<ProductEntity> query = context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.SellerId == sellerId);

        var totalCount = await query.CountAsync(cancellationToken);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var productDtos = mapper.Map<IEnumerable<ProductDto>>(products).ToList();

        return new PagedResult<ProductDto>
        {
            Items = productDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<SellerPerformanceDto> GetPerformanceMetricsAsync(Guid sellerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        var totalSales = await (
            from o in context.Set<OrderEntity>().AsNoTracking()
            join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            where o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  p.SellerId == sellerId
            select oi.TotalPrice
        ).SumAsync(cancellationToken);

        var totalOrders = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .CountAsync(cancellationToken);

        var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;

        // Sales by date
        var salesByDate = await (
            from o in context.Set<OrderEntity>().AsNoTracking()
            join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            where o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  p.SellerId == sellerId
            group new { o, oi } by o.CreatedAt.Date into g
            select new SalesByDateDto
            {
                Date = g.Key,
                Sales = g.Sum(x => x.oi.TotalPrice),
                OrderCount = g.Select(x => x.o.Id).Distinct().Count()
            }
        ).OrderBy(s => s.Date).ToListAsync(cancellationToken);

        // Top products
        var topProducts = await context.Set<OrderItem>()
            .AsNoTracking()
            .Include(oi => oi.Product)
            .Where(oi => oi.Order.PaymentStatus == PaymentStatus.Completed &&
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
            .Take(sellerConfig.TopProductsLimit)
            .ToListAsync(cancellationToken);

        var sellerProfile = await context.Set<SellerProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.UserId == sellerId, cancellationToken);

        var uniqueCustomers = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .Select(o => o.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

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

    public async Task<SellerPerformanceMetricsDto> GetDetailedPerformanceMetricsAsync(Guid sellerId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var periodDays = (endDate - startDate).Days;
        var previousStartDate = startDate.AddDays(-periodDays);
        var previousEndDate = startDate;

        // Sales metrics
        var totalSales = await (
            from o in context.Set<OrderEntity>().AsNoTracking()
            join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            where o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  p.SellerId == sellerId
            select oi.TotalPrice
        ).SumAsync(cancellationToken);

        var previousSales = await (
            from o in context.Set<OrderEntity>().AsNoTracking()
            join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            where o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= previousStartDate && o.CreatedAt < previousEndDate &&
                  p.SellerId == sellerId
            select oi.TotalPrice
        ).SumAsync(cancellationToken);

        var salesGrowth = previousSales > 0 ? ((totalSales - previousSales) / previousSales) * 100 : 0;

        var totalOrders = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .CountAsync(cancellationToken);

        var previousOrdersCount = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= previousStartDate && o.CreatedAt < previousEndDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .CountAsync(cancellationToken);

        var orderGrowth = previousOrdersCount > 0 ? ((decimal)(totalOrders - previousOrdersCount) / previousOrdersCount) * 100 : 0;

        var averageOrderValue = totalOrders > 0 ? totalSales / totalOrders : 0;
        var previousAOV = previousOrdersCount > 0 ? previousSales / previousOrdersCount : 0;

        // Customer metrics
        var currentCustomerIds = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .Select(o => o.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var previousCustomerIds = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= previousStartDate && o.CreatedAt < previousEndDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .Select(o => o.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var totalCustomers = currentCustomerIds.Count;
        
        // Note: EF Core'da EXCEPT/INTERSECT direkt desteklenmiyor, bu yüzden küçük listeler için memory'de yapılması gerekebilir
        // Ama mümkün olduğunca database'de yapıyoruz
        var newCustomers = currentCustomerIds.Except(previousCustomerIds).Count();
        var returningCustomers = currentCustomerIds.Intersect(previousCustomerIds).Count();
        var customerRetentionRate = previousCustomerIds.Count > 0 ? (decimal)returningCustomers / previousCustomerIds.Count * 100 : 0;

        // Product metrics
        var totalProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.SellerId == sellerId, cancellationToken);

        var activeProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.SellerId == sellerId && p.IsActive, cancellationToken);

        var lowStockProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.SellerId == sellerId && p.IsActive && p.StockQuantity <= sellerConfig.LowStockThreshold, cancellationToken);

        var outOfStockProducts = await context.Set<ProductEntity>()
            .AsNoTracking()
            .CountAsync(p => p.SellerId == sellerId && p.IsActive && p.StockQuantity == 0, cancellationToken);

        var totalReviews = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.Product)
            .Where(r => r.IsApproved && r.Product.SellerId == sellerId)
            .CountAsync(cancellationToken);

        var averageProductRating = await context.Set<ReviewEntity>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(r => r.Product)
            .Where(r => r.IsApproved && r.Product.SellerId == sellerId)
            .AverageAsync(r => (double?)r.Rating, cancellationToken) ?? 0;

        // Fulfillment metrics - Note: Bu karmaşık hesaplamalar için bazı order'ları yüklemek gerekebilir
        // Ama mümkün olduğunca database'de yapıyoruz
        var averageFulfillmentTime = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(o => o.Shipping)
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId) &&
                  o.Shipping != null && o.Shipping.Status == ShippingStatus.Shipped && o.DeliveredDate.HasValue)
            .AverageAsync(o => (double?)(o.DeliveredDate!.Value - (o.ShippedDate ?? o.CreatedAt)).TotalHours, cancellationToken) ?? 0;

        // Shipping time metrics
        var averageShippingTime = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Include(o => o.Shipping)
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId) &&
                  o.Shipping != null && o.Shipping.Status == ShippingStatus.Shipped && o.ShippedDate.HasValue)
            .AverageAsync(o => (double?)(o.ShippedDate!.Value - o.CreatedAt).TotalHours, cancellationToken) ?? 0;

        // Return & Refund metrics
        var totalReturns = await (
            from r in context.Set<ReturnRequest>().AsNoTracking()
            join o in context.Set<OrderEntity>().AsNoTracking() on r.OrderId equals o.Id
            join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            where p.SellerId == sellerId &&
                  r.CreatedAt >= startDate && r.CreatedAt <= endDate
            select r.Id
        ).Distinct().CountAsync(cancellationToken);

        var returnRate = totalOrders > 0 ? (decimal)totalReturns / totalOrders * 100 : 0;

        var totalRefunds = await (
            from r in context.Set<ReturnRequest>().AsNoTracking()
            join o in context.Set<OrderEntity>().AsNoTracking() on r.OrderId equals o.Id
            join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            where p.SellerId == sellerId &&
                  r.CreatedAt >= startDate && r.CreatedAt <= endDate &&
                  r.Status == ReturnRequestStatus.Approved
            select r.RefundAmount
        ).SumAsync(cancellationToken);

        var refundRate = totalSales > 0 ? (totalRefunds / totalSales) * 100 : 0;

        // Conversion metrics (simplified - would need view tracking)
        var productViews = await context.Set<UserActivityLog>()
            .AsNoTracking()
            .Where(a => a.ActivityType == ActivityType.ViewProduct && 
                  a.CreatedAt >= startDate && a.CreatedAt <= endDate &&
                  a.EntityType == EntityType.Product)
            .Join(context.Set<ProductEntity>().AsNoTracking().Where(p => p.SellerId == sellerId),
                  activity => activity.EntityId,
                  product => product.Id,
                  (activity, product) => activity)
            .CountAsync(cancellationToken);
        var addToCarts = await context.Set<CartItem>()
            .AsNoTracking()
            .Include(ci => ci.Product)
            .Where(ci => ci.Product.SellerId == sellerId &&
                  ci.CreatedAt >= startDate && ci.CreatedAt <= endDate)
            .CountAsync(cancellationToken);
        var conversionRate = productViews > 0 ? (decimal)totalOrders / productViews * 100 : 0;
        var cartAbandonmentRate = addToCarts > 0 ? ((decimal)(addToCarts - totalOrders) / addToCarts) * 100 : 0;

        // Category performance
        var categoryPerformance = await (
            from o in context.Set<OrderEntity>().AsNoTracking()
            join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            join c in context.Set<Category>().AsNoTracking() on p.CategoryId equals c.Id
            where o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  p.SellerId == sellerId
            group new { oi, c } by new { CategoryId = c.Id, CategoryName = c.Name } into g
            select new CategoryPerformanceDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                ProductCount = g.Select(x => x.oi.ProductId).Distinct().Count(),
                OrdersCount = g.Select(x => x.oi.OrderId).Distinct().Count(),
                Revenue = g.Sum(x => x.oi.TotalPrice),
                AverageRating = 0 // Would need to calculate from reviews
            }
        ).OrderByDescending(c => c.Revenue).ToListAsync(cancellationToken);

        // Sales trends
        var salesTrends = await (
            from o in context.Set<OrderEntity>().AsNoTracking()
            join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            where o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  p.SellerId == sellerId
            group new { o, oi } by o.CreatedAt.Date into g
            let totalSalesAmount = g.Sum(x => x.oi.TotalPrice)
            let orderCount = g.Select(x => x.o.Id).Distinct().Count()
            select new SalesTrendDto
            {
                Date = g.Key,
                Sales = totalSalesAmount,
                OrderCount = orderCount,
                AverageOrderValue = orderCount > 0 ? totalSalesAmount / orderCount : 0
            }
        ).OrderBy(t => t.Date).ToListAsync(cancellationToken);

        // Order trends
        var orderTrends = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  o.OrderItems.Any(oi => oi.Product.SellerId == sellerId))
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new OrderTrendDto
            {
                Date = g.Key,
                OrderCount = g.Count(),
                CompletedOrders = g.Count(o => o.Status == OrderStatus.Delivered),
                CancelledOrders = g.Count(o => o.Status == OrderStatus.Cancelled)
            })
            .OrderBy(t => t.Date)
            .ToListAsync(cancellationToken);

        // Top/Worst products
        var topProducts = await context.Set<OrderItem>()
            .AsNoTracking()
            .Include(oi => oi.Product)
            .Where(oi => oi.Order.PaymentStatus == PaymentStatus.Completed &&
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
            .Take(sellerConfig.TopProductsLimit)
            .ToListAsync(cancellationToken);

        var worstProducts = await context.Set<OrderItem>()
            .AsNoTracking()
            .Include(oi => oi.Product)
            .Where(oi => oi.Order.PaymentStatus == PaymentStatus.Completed &&
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
            .Take(sellerConfig.TopProductsLimit)
            .ToListAsync(cancellationToken);

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

    public async Task<List<CategoryPerformanceDto>> GetCategoryPerformanceAsync(Guid sellerId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        startDate ??= DateTime.UtcNow.AddDays(-30);
        endDate ??= DateTime.UtcNow;

        return await (
            from o in context.Set<OrderEntity>().AsNoTracking()
            join oi in context.Set<OrderItem>().AsNoTracking() on o.Id equals oi.OrderId
            join p in context.Set<ProductEntity>().AsNoTracking() on oi.ProductId equals p.Id
            join c in context.Set<Category>().AsNoTracking() on p.CategoryId equals c.Id
            where o.PaymentStatus == PaymentStatus.Completed &&
                  o.CreatedAt >= startDate && o.CreatedAt <= endDate &&
                  p.SellerId == sellerId
            group new { oi, c } by new { CategoryId = c.Id, CategoryName = c.Name } into g
            let orderCount = g.Select(x => x.oi.OrderId).Distinct().Count()
            select new CategoryPerformanceDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                ProductCount = g.Select(x => x.oi.ProductId).Distinct().Count(),
                OrderCount = orderCount,
                OrdersCount = orderCount, // ✅ PERFORMANCE FIX: Redundant Distinct().Count() hesaplaması düzeltildi
                Revenue = g.Sum(x => x.oi.TotalPrice),
                AverageRating = 0
            })
            .OrderByDescending(c => c.Revenue)
            .ToListAsync(cancellationToken);
    }
}

