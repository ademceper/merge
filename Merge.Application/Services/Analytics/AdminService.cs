using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Interfaces.Analytics;
using Merge.Application.Interfaces.Catalog;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Order;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Review;
using Merge.Application.DTOs.User;
using Merge.Application.Common;


namespace Merge.Application.Services.Analytics;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<AdminService> _logger;
    private readonly AnalyticsSettings _settings;

    public AdminService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IInventoryService inventoryService,
        ILogger<AdminService> logger,
        IOptions<AnalyticsSettings> settings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _inventoryService = inventoryService;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching dashboard statistics");

        var stats = new DashboardStatsDto
        {
            TotalUsers = await _context.Users.AsNoTracking().CountAsync(cancellationToken),
            ActiveUsers = await _context.Users.AsNoTracking().CountAsync(u => u.EmailConfirmed, cancellationToken),
            TotalProducts = await _context.Products.AsNoTracking().CountAsync(cancellationToken),
            ActiveProducts = await _context.Products.AsNoTracking().CountAsync(p => p.IsActive, cancellationToken),
            TotalOrders = await _context.Orders.AsNoTracking().CountAsync(cancellationToken),
            TotalRevenue = await _context.Orders
                .AsNoTracking()
                .Where(o => o.PaymentStatus == PaymentStatus.Completed)
                .SumAsync(o => o.TotalAmount, cancellationToken),
            PendingOrders = await _context.Orders.AsNoTracking().CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken),
            TodayOrders = await _context.Orders.AsNoTracking().CountAsync(o => o.CreatedAt.Date == DateTime.UtcNow.Date, cancellationToken),
            TodayRevenue = await _context.Orders
                .AsNoTracking()
                .Where(o => o.PaymentStatus == PaymentStatus.Completed && o.CreatedAt.Date == DateTime.UtcNow.Date)
                .SumAsync(o => o.TotalAmount, cancellationToken),
            TotalWarehouses = await _context.Warehouses.AsNoTracking().CountAsync(cancellationToken),
            ActiveWarehouses = await _context.Warehouses.AsNoTracking().CountAsync(w => w.IsActive, cancellationToken),
            // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
            LowStockProducts = await _context.Products.AsNoTracking().CountAsync(p => p.StockQuantity <= _settings.LowStockThreshold, cancellationToken),
            TotalCategories = await _context.Categories.AsNoTracking().CountAsync(cancellationToken),
            PendingReviews = await _context.Reviews.AsNoTracking().CountAsync(r => !r.IsApproved, cancellationToken),
            PendingReturns = await _context.ReturnRequests.AsNoTracking().CountAsync(r => r.Status == ReturnRequestStatus.Pending, cancellationToken),
            Users2FAEnabled = await _context.Set<TwoFactorAuth>().AsNoTracking().CountAsync(t => t.IsEnabled, cancellationToken)
        };

        _logger.LogInformation("Dashboard statistics fetched successfully");
        return stats;
    }

    public async Task<RevenueChartDto> GetRevenueChartAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        // ✅ PERFORMANCE: Database'de toplam hesapla (memory'de Sum YASAK)
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted check (Global Query Filter handles it)
        var ordersQuery = _context.Orders
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed && o.CreatedAt >= startDate);

        var dailyRevenue = await ordersQuery
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new DailyRevenueDto
            {
                Date = g.Key,
                Revenue = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count()
            })
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de toplam hesapla (memory'de Sum yerine)
        var totalRevenue = await ordersQuery.SumAsync(o => o.TotalAmount, cancellationToken);
        var totalOrders = await ordersQuery.CountAsync(cancellationToken);

        var chart = new RevenueChartDto
        {
            Days = days,
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            DailyData = dailyRevenue
        };

        return chart;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<AdminTopProductDto>> GetTopProductsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !oi.IsDeleted check (Global Query Filter handles it)
        var topProducts = await _context.OrderItems
            .AsNoTracking()
            .Include(oi => oi.Product)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name, oi.Product.ImageUrl })
            .Select(g => new AdminTopProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                ImageUrl = g.Key.ImageUrl,
                TotalSold = g.Sum(oi => oi.Quantity),
                TotalRevenue = g.Sum(oi => oi.TotalPrice)
            })
            .OrderByDescending(p => p.TotalSold)
            .Take(count)
            .ToListAsync(cancellationToken);

        return topProducts;
    }

    public async Task<InventoryOverviewDto> GetInventoryOverviewAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Materialize IEnumerable to avoid re-enumeration
        var lowStockAlerts = (await _inventoryService.GetLowStockAlertsAsync()).ToList();
        
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !i.IsDeleted checks (Global Query Filter handles it)
        var totalInventoryValue = await _context.Inventories
            .AsNoTracking()
            .SumAsync(i => i.Quantity * i.UnitCost, cancellationToken);

        var overview = new InventoryOverviewDto
        {
            TotalWarehouses = await _context.Warehouses.AsNoTracking().CountAsync(w => w.IsActive, cancellationToken),
            TotalInventoryItems = await _context.Inventories.AsNoTracking().CountAsync(cancellationToken),
            TotalInventoryValue = totalInventoryValue,
            LowStockCount = lowStockAlerts.Count,  // ✅ List.Count (re-enumeration yok)
            LowStockAlerts = lowStockAlerts.Take(5).ToList(),  // ✅ List üzerinde işlem (re-enumeration yok)
            TotalStockQuantity = await _context.Inventories.AsNoTracking().SumAsync(i => i.Quantity, cancellationToken),
            ReservedStockQuantity = await _context.Inventories.AsNoTracking().SumAsync(i => i.ReservedQuantity, cancellationToken)
        };

        return overview;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<OrderDto>> GetRecentOrdersAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted check (Global Query Filter handles it)
        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync(int threshold = 10, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !p.IsDeleted check (Global Query Filter handles it)
        var products = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.StockQuantity <= threshold && p.IsActive)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndür
    public async Task<PagedResult<ReviewDto>> GetPendingReviewsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted check (Global Query Filter handles it)
        var query = _context.Reviews
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => !r.IsApproved);

        var totalCount = await query.CountAsync(cancellationToken);

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return new PagedResult<ReviewDto>
        {
            Items = _mapper.Map<List<ReviewDto>>(reviews),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndür
    public async Task<PagedResult<ReturnRequestDto>> GetPendingReturnsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted check (Global Query Filter handles it)
        var query = _context.ReturnRequests
            .AsNoTracking()
            .Include(r => r.User)
            .Include(r => r.Order)
            .Where(r => r.Status == ReturnRequestStatus.Pending);

        var totalCount = await query.CountAsync(cancellationToken);

        var returns = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return new PagedResult<ReturnRequestDto>
        {
            Items = _mapper.Map<List<ReturnRequestDto>>(returns),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndür
    public async Task<PagedResult<UserDto>> GetUsersAsync(int page = 1, int pageSize = 20, string? role = null, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !u.IsDeleted check (Global Query Filter handles it)
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrEmpty(role))
        {
            query = query.Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id && 
                _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == role)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return new PagedResult<UserDto>
        {
            Items = _mapper.Map<List<UserDto>>(users),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> ActivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ FIX: Use FirstOrDefaultAsync instead of FindAsync to respect Global Query Filter
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            return false;
        }

        user.EmailConfirmed = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ FIX: Use FirstOrDefaultAsync instead of FindAsync to respect Global Query Filter
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            return false;
        }

        user.EmailConfirmed = false;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ChangeUserRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default)
    {
        // ✅ FIX: Use FirstOrDefaultAsync instead of FindAsync to respect Global Query Filter
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            return false;
        }

        // Remove existing roles
        // ⚠️ NOTE: AsNoTracking removed - we need to track entities for RemoveRange
        var existingRoles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync(cancellationToken);
        _context.UserRoles.RemoveRange(existingRoles);
        
        // Add new role
        // ✅ PERFORMANCE: AsNoTracking for read-only queries (we don't modify this entity)
        var roleEntity = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Name == role, cancellationToken);
        if (roleEntity != null)
        {
            await _context.UserRoles.AddAsync(new IdentityUserRole<Guid>
            {
                UserId = userId,
                RoleId = roleEntity.Id
            }, cancellationToken);
        }
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ FIX: Use FirstOrDefaultAsync instead of FindAsync to respect Global Query Filter
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            return false;
        }

        user.IsDeleted = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        // ✅ PERFORMANCE: Removed manual !IsDeleted checks (Global Query Filter handles it)
        var summary = new AnalyticsSummaryDto
        {
            Period = $"Last {days} days",
            NewUsers = await _context.Users.AsNoTracking().CountAsync(u => u.CreatedAt >= startDate, cancellationToken),
            NewOrders = await _context.Orders.AsNoTracking().CountAsync(o => o.CreatedAt >= startDate, cancellationToken),
            Revenue = await _context.Orders
                .AsNoTracking()
                .Where(o => o.PaymentStatus == PaymentStatus.Completed && o.CreatedAt >= startDate)
                .SumAsync(o => o.TotalAmount, cancellationToken),
            AverageOrderValue = await _context.Orders
                .AsNoTracking()
                .Where(o => o.CreatedAt >= startDate)
                .AverageAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0,
            NewProducts = await _context.Products.AsNoTracking().CountAsync(p => p.CreatedAt >= startDate, cancellationToken),
            TotalReviews = await _context.Reviews.AsNoTracking().CountAsync(r => r.CreatedAt >= startDate, cancellationToken),
            AverageRating = await _context.Reviews
                .AsNoTracking()
                .Where(r => r.CreatedAt >= startDate)
                .AverageAsync(r => (decimal?)r.Rating, cancellationToken) ?? 0
        };

        return summary;
    }

    public async Task<TwoFactorStatsDto> Get2FAStatsAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Database'de toplam hesapla (memory'de Sum YASAK)
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !u.IsDeleted and !t.IsDeleted checks (Global Query Filter handles it)
        var totalUsers = await _context.Users.AsNoTracking().CountAsync(cancellationToken);
        
        var twoFactorQuery = _context.Set<TwoFactorAuth>()
            .AsNoTracking()
            .Where(t => t.IsEnabled);

        var usersWithTwoFactorCount = await twoFactorQuery.CountAsync(cancellationToken);
        
        var usersWithTwoFactor = await twoFactorQuery
            .GroupBy(t => t.Method)
            .Select(g => new TwoFactorMethodCount
            {
                Method = g.Key.ToString(),
                Count = g.Count()
            })
            .ToListAsync(cancellationToken);

        var stats = new TwoFactorStatsDto
        {
            TotalUsers = totalUsers,
            UsersWithTwoFactor = usersWithTwoFactorCount,
            TwoFactorPercentage = totalUsers > 0 ? (usersWithTwoFactorCount * 100.0m / totalUsers) : 0,
            MethodBreakdown = usersWithTwoFactor
        };

        return stats;
    }

    public async Task<SystemHealthDto> GetSystemHealthAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var health = new SystemHealthDto
        {
            DatabaseStatus = "Connected",
            TotalRecords = await _context.Users.AsNoTracking().CountAsync(cancellationToken) +
                          await _context.Products.AsNoTracking().CountAsync(cancellationToken) +
                          await _context.Orders.AsNoTracking().CountAsync(cancellationToken),
            LastBackup = DateTime.UtcNow.AddDays(-1), // Mock data
            DiskUsage = "45%", // Mock data
            MemoryUsage = "62%", // Mock data
            ActiveSessions = 125 // Mock data
        };

        return health;
    }
}

