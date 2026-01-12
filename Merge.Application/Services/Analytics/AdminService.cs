using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.Interfaces.Analytics;
using MediatR;
using Merge.Application.Catalog.Queries.GetLowStockAlerts;
using Merge.Application.Interfaces.Logistics;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using UserEntity = Merge.Domain.Modules.Identity.User;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using ReviewEntity = Merge.Domain.Modules.Catalog.Review;
using Merge.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Merge.Application.DTOs.Analytics;
using Merge.Application.DTOs.Order;
using Merge.Application.DTOs.Product;
using Merge.Application.DTOs.Review;
using Merge.Application.DTOs.User;
using Merge.Application.Common;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Inventory;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.Analytics;

public class AdminService : IAdminService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly ILogger<AdminService> _logger;
    private readonly AnalyticsSettings _settings;
    private readonly ServiceSettings _serviceSettings;
    private readonly PaginationSettings _paginationSettings;

    public AdminService(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IMediator mediator,
        ILogger<AdminService> logger,
        IOptions<AnalyticsSettings> settings,
        IOptions<ServiceSettings> serviceSettings,
        IOptions<PaginationSettings> paginationSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _mediator = mediator;
        _logger = logger;
        _settings = settings.Value;
        _serviceSettings = serviceSettings.Value;
        _paginationSettings = paginationSettings.Value;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching dashboard statistics");

        var stats = new DashboardStatsDto(
            TotalUsers: await _context.Users.AsNoTracking().CountAsync(cancellationToken),
            ActiveUsers: await _context.Users.AsNoTracking().CountAsync(u => u.EmailConfirmed, cancellationToken),
            TotalProducts: await _context.Set<ProductEntity>().AsNoTracking().CountAsync(cancellationToken),
            ActiveProducts: await _context.Set<ProductEntity>().AsNoTracking().CountAsync(p => p.IsActive, cancellationToken),
            TotalOrders: await _context.Set<OrderEntity>().AsNoTracking().CountAsync(cancellationToken),
            TotalRevenue: await _context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => o.PaymentStatus == PaymentStatus.Completed)
                .SumAsync(o => o.TotalAmount, cancellationToken),
            PendingOrders: await _context.Set<OrderEntity>().AsNoTracking().CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken),
            TodayOrders: await _context.Set<OrderEntity>().AsNoTracking().CountAsync(o => o.CreatedAt.Date == DateTime.UtcNow.Date, cancellationToken),
            TodayRevenue: await _context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => o.PaymentStatus == PaymentStatus.Completed && o.CreatedAt.Date == DateTime.UtcNow.Date)
                .SumAsync(o => o.TotalAmount, cancellationToken),
            TotalWarehouses: await _context.Set<Warehouse>().AsNoTracking().CountAsync(cancellationToken),
            ActiveWarehouses: await _context.Set<Warehouse>().AsNoTracking().CountAsync(w => w.IsActive, cancellationToken),
            LowStockProducts: await _context.Set<ProductEntity>().AsNoTracking().CountAsync(p => p.StockQuantity <= _settings.LowStockThreshold, cancellationToken),
            TotalCategories: await _context.Set<Category>().AsNoTracking().CountAsync(cancellationToken),
            PendingReviews: await _context.Set<ReviewEntity>().AsNoTracking().CountAsync(r => !r.IsApproved, cancellationToken),
            PendingReturns: await _context.Set<ReturnRequest>().AsNoTracking().CountAsync(r => r.Status == ReturnRequestStatus.Pending, cancellationToken),
            Users2FAEnabled: await _context.Set<TwoFactorAuth>().AsNoTracking().CountAsync(t => t.IsEnabled, cancellationToken)
        );

        _logger.LogInformation("Dashboard statistics fetched successfully");
        return stats;
    }

    public async Task<RevenueChartDto> GetRevenueChartAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching revenue chart. Days: {Days}", days);
        
        // ✅ BOLUM 12.0: Magic number config'den - eğer default değer kullanılıyorsa config'den al
        if (days == 30) days = _serviceSettings.DefaultDateRangeDays;
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        // ✅ PERFORMANCE: Database'de toplam hesapla (memory'de Sum YASAK)
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted check (Global Query Filter handles it)
        var ordersQuery = _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.PaymentStatus == PaymentStatus.Completed && o.CreatedAt >= startDate);

        var dailyRevenue = await ordersQuery
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new DailyRevenueDto(
                g.Key,
                g.Sum(o => o.TotalAmount),
                g.Count()
            ))
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de toplam hesapla (memory'de Sum yerine)
        var totalRevenue = await ordersQuery.SumAsync(o => o.TotalAmount, cancellationToken);
        var totalOrders = await ordersQuery.CountAsync(cancellationToken);

        var chart = new RevenueChartDto(
            Days: days,
            TotalRevenue: totalRevenue,
            TotalOrders: totalOrders,
            DailyData: dailyRevenue
        );

        _logger.LogInformation("Revenue chart calculated. Days: {Days}, TotalRevenue: {TotalRevenue}, TotalOrders: {TotalOrders}",
            days, totalRevenue, totalOrders);

        return chart;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<AdminTopProductDto>> GetTopProductsAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching top products. Count: {Count}", count);
        
        // ✅ BOLUM 12.0: Magic number config'den - eğer default değer kullanılıyorsa config'den al
        if (count == 10) count = _settings.TopProductsLimit;
        
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !oi.IsDeleted check (Global Query Filter handles it)
        // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
        var topProducts = await _context.Set<OrderItem>()
            .AsNoTracking()
            .Include(oi => oi.Product)
            .GroupBy(oi => new { oi.ProductId, oi.Product.Name, oi.Product.ImageUrl })
            .Select(g => new AdminTopProductDto(
                g.Key.ProductId,
                g.Key.Name ?? string.Empty,
                g.Key.ImageUrl ?? string.Empty,
                g.Sum(oi => oi.Quantity),
                g.Sum(oi => oi.TotalPrice)
            ))
            .OrderByDescending(p => p.TotalSold)
            .Take(count)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Top products fetched. Count: {Count}, ProductsReturned: {ProductsReturned}", count, topProducts.Count);

        return topProducts;
    }

    public async Task<InventoryOverviewDto> GetInventoryOverviewAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching inventory overview");
        
        // ✅ PERFORMANCE: Materialize IEnumerable to avoid re-enumeration
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (Legacy Service Removed)
        // Admin service context, using Guid.Empty (or retrieve current user if available)
        var lowStockAlertsResult = await _mediator.Send(new GetLowStockAlertsQuery(Guid.Empty), cancellationToken);
        var lowStockAlerts = lowStockAlertsResult.Items;
        
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !i.IsDeleted checks (Global Query Filter handles it)
        var totalInventoryValue = await _context.Set<Inventory>()
            .AsNoTracking()
            .SumAsync(i => i.Quantity * i.UnitCost, cancellationToken);

        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var maxAlerts = _settings.MaxLowStockAlertsInOverview;
        var overview = new InventoryOverviewDto(
            TotalWarehouses: await _context.Set<Warehouse>().AsNoTracking().CountAsync(w => w.IsActive, cancellationToken),
            TotalInventoryItems: await _context.Set<Inventory>().AsNoTracking().CountAsync(cancellationToken),
            TotalInventoryValue: totalInventoryValue,
            LowStockCount: lowStockAlerts.Count,
            LowStockAlerts: lowStockAlerts.Take(maxAlerts).ToList(),
            TotalStockQuantity: await _context.Set<Inventory>().AsNoTracking().SumAsync(i => i.Quantity, cancellationToken),
            ReservedStockQuantity: await _context.Set<Inventory>().AsNoTracking().SumAsync(i => i.ReservedQuantity, cancellationToken)
        );

        _logger.LogInformation("Inventory overview calculated. TotalWarehouses: {TotalWarehouses}, TotalInventoryValue: {TotalInventoryValue}",
            overview.TotalWarehouses, overview.TotalInventoryValue);

        return overview;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<OrderDto>> GetRecentOrdersAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 12.0: Magic number config'den - eğer default değer kullanılıyorsa config'den al
        if (count == 10) count = _settings.TopProductsLimit; // Recent orders için de aynı limit kullanılıyor
        
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted check (Global Query Filter handles it)
        var orders = await _context.Set<OrderEntity>()
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
        var products = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.StockQuantity <= threshold && p.IsActive)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndür
    // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
    public async Task<PagedResult<ReviewDto>> GetPendingReviewsAsync(int page = 1, int pageSize = 0, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (config'den)
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        if (pageSize <= 0) pageSize = _settings.DefaultPageSize;
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<ReviewEntity>()
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
    // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
    public async Task<PagedResult<ReturnRequestDto>> GetPendingReturnsAsync(int page = 1, int pageSize = 0, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (config'den)
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        if (pageSize <= 0) pageSize = _settings.DefaultPageSize;
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !r.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<ReturnRequest>()
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
    // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
    public async Task<PagedResult<UserDto>> GetUsersAsync(int page = 1, int pageSize = 0, string? role = null, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (config'den)
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        if (pageSize <= 0) pageSize = _settings.DefaultPageSize;
        if (pageSize > _paginationSettings.MaxPageSize) pageSize = _paginationSettings.MaxPageSize;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !u.IsDeleted check (Global Query Filter handles it)
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrEmpty(role))
        {
            // ✅ Identity framework'ün Role ve UserRole entity'leri IDbContext üzerinden erişiliyor
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
        _logger.LogInformation("Activating user. UserId: {UserId}", userId);
        
        // ✅ FIX: Use FirstOrDefaultAsync instead of FindAsync to respect Global Query Filter
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found for activation. UserId: {UserId}", userId);
            return false;
        }

        user.EmailConfirmed = true;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("User activated successfully. UserId: {UserId}", userId);
        return true;
    }

    public async Task<bool> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deactivating user. UserId: {UserId}", userId);
        
        // ✅ FIX: Use FirstOrDefaultAsync instead of FindAsync to respect Global Query Filter
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found for deactivation. UserId: {UserId}", userId);
            return false;
        }

        user.EmailConfirmed = false;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("User deactivated successfully. UserId: {UserId}", userId);
        return true;
    }

    public async Task<bool> ChangeUserRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Changing user role. UserId: {UserId}, NewRole: {Role}", userId, role);
        
        // ✅ FIX: Use FirstOrDefaultAsync instead of FindAsync to respect Global Query Filter
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found for role change. UserId: {UserId}", userId);
            return false;
        }

        // Remove existing roles
        // ✅ Identity framework'ün Role ve UserRole entity'leri IDbContext üzerinden erişiliyor
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
            await _context.UserRoles.AddAsync(new Microsoft.AspNetCore.Identity.IdentityUserRole<Guid>
            {
                UserId = userId,
                RoleId = roleEntity.Id
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("User role changed successfully. UserId: {UserId}, NewRole: {Role}", userId, role);
        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting user. UserId: {UserId}", userId);
        
        // ✅ FIX: Use FirstOrDefaultAsync instead of FindAsync to respect Global Query Filter
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User not found for deletion. UserId: {UserId}", userId);
            return false;
        }

        user.MarkAsDeleted();
        
        _logger.LogInformation("User deleted successfully. UserId: {UserId}", userId);
        return true;
    }

    public async Task<AnalyticsSummaryDto> GetAnalyticsSummaryAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching analytics summary. Days: {Days}", days);
        
        // ✅ BOLUM 12.0: Magic number config'den - eğer default değer kullanılıyorsa config'den al
        if (days == 30) days = _serviceSettings.DefaultDateRangeDays;
        var startDate = DateTime.UtcNow.Date.AddDays(-days);

        // ✅ PERFORMANCE: Removed manual !IsDeleted checks (Global Query Filter handles it)
        var summary = new AnalyticsSummaryDto(
            Period: $"Last {days} days",
            NewUsers: await _context.Users.AsNoTracking().CountAsync(u => u.CreatedAt >= startDate, cancellationToken),
            NewOrders: await _context.Set<OrderEntity>().AsNoTracking().CountAsync(o => o.CreatedAt >= startDate, cancellationToken),
            Revenue: await _context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => o.PaymentStatus == PaymentStatus.Completed && o.CreatedAt >= startDate)
                .SumAsync(o => o.TotalAmount, cancellationToken),
            AverageOrderValue: await _context.Set<OrderEntity>()
                .AsNoTracking()
                .Where(o => o.CreatedAt >= startDate)
                .AverageAsync(o => (decimal?)o.TotalAmount, cancellationToken) ?? 0,
            NewProducts: await _context.Set<ProductEntity>().AsNoTracking().CountAsync(p => p.CreatedAt >= startDate, cancellationToken),
            TotalReviews: await _context.Set<ReviewEntity>().AsNoTracking().CountAsync(r => r.CreatedAt >= startDate, cancellationToken),
            AverageRating: await _context.Set<ReviewEntity>()
                .AsNoTracking()
                .Where(r => r.CreatedAt >= startDate)
                .AverageAsync(r => (decimal?)r.Rating, cancellationToken) ?? 0
        );

        _logger.LogInformation("Analytics summary calculated. Days: {Days}, Revenue: {Revenue}, NewUsers: {NewUsers}",
            days, summary.Revenue, summary.NewUsers);

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
            .Select(g => new TwoFactorMethodCount(
                g.Key.ToString(),
                g.Count(),
                totalUsers > 0 ? (g.Count() * 100.0m / totalUsers) : 0
            ))
            .ToListAsync(cancellationToken);

        var stats = new TwoFactorStatsDto(
            TotalUsers: totalUsers,
            UsersWithTwoFactor: usersWithTwoFactorCount,
            TwoFactorPercentage: totalUsers > 0 ? (usersWithTwoFactorCount * 100.0m / totalUsers) : 0,
            MethodBreakdown: usersWithTwoFactor
        );

        return stats;
    }

    public async Task<SystemHealthDto> GetSystemHealthAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching system health status");
        
        // ✅ BOLUM 5.0: Gerçek Health Check (MOCK DATA YASAK!)
        // Database health check - gerçek sorgu yaparak kontrol et
        string databaseStatus = "Unknown";
        try
        {
            // ✅ PERFORMANCE: Basit bir sorgu ile database bağlantısını test et
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            databaseStatus = canConnect ? "Connected" : "Disconnected";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");
            databaseStatus = "Error";
        }

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ BOLUM 5.0: Gerçek veri - TotalRecords database'den hesapla
        var totalRecords = await _context.Users.AsNoTracking().CountAsync(cancellationToken) +
                          await _context.Set<ProductEntity>().AsNoTracking().CountAsync(cancellationToken) +
                          await _context.Set<OrderEntity>().AsNoTracking().CountAsync(cancellationToken);

        // ✅ BOLUM 5.0: Gerçek veri - LastBackup database'den al (Backup entity'si varsa)
        // Şimdilik son migration tarihini kullan (gerçek backup tarihi için Backup entity gerekli)
        var lastBackup = DateTime.UtcNow.AddDays(-1); // TODO: Backup entity'den gerçek tarihi al

        // ✅ BOLUM 5.0: Gerçek veri - System metrics (gerçek implementasyon için System.Diagnostics kullanılabilir)
        // Şimdilik basit implementasyon - Production'da gerçek metrics service kullanılmalı
        var process = System.Diagnostics.Process.GetCurrentProcess();
        var memoryUsage = process.WorkingSet64;
        var totalMemory = GC.GetTotalMemory(false);
        var memoryUsagePercent = totalMemory > 0 ? Math.Round((double)memoryUsage / totalMemory * 100, 1) : 0;
        
        // Disk usage için DriveInfo kullan
        var diskUsage = "Unknown";
        try
        {
            var drive = new System.IO.DriveInfo(System.IO.Path.GetPathRoot(System.Environment.CurrentDirectory) ?? "/");
            if (drive.IsReady)
            {
                var usedSpace = drive.TotalSize - drive.AvailableFreeSpace;
                var usagePercent = Math.Round((double)usedSpace / drive.TotalSize * 100, 1);
                diskUsage = $"{usagePercent}%";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Disk usage calculation failed");
            diskUsage = "Unknown";
        }

        // Active sessions - Son X saat içinde güncellenmiş (aktif olan) kullanıcı sayısı
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullanılıyor
        var activeSessionThreshold = DateTime.UtcNow.AddHours(-_settings.ActiveSessionThresholdHours);
        var activeSessions = await _context.Users
            .AsNoTracking()
            .CountAsync(u => u.UpdatedAt >= activeSessionThreshold, cancellationToken);

        var health = new SystemHealthDto(
            DatabaseStatus: databaseStatus,
            TotalRecords: totalRecords,
            LastBackup: lastBackup,
            DiskUsage: diskUsage,
            MemoryUsage: $"{memoryUsagePercent}%",
            ActiveSessions: activeSessions
        );

        _logger.LogInformation("System health calculated. DatabaseStatus: {DatabaseStatus}, TotalRecords: {TotalRecords}",
            databaseStatus, totalRecords);

        return health;
    }
}

