using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserEntity = Merge.Domain.Modules.Identity.User;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Domain.Entities;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Application.DTOs.User;
using Microsoft.Extensions.Logging;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Services.User;

public class UserActivityService : IUserActivityService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserActivityService> _logger;

    public UserActivityService(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UserActivityService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task LogActivityAsync(CreateActivityLogDto activityDto, string ipAddress, string userAgent, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Logging activity: {ActivityType} for user: {UserId}", activityDto.ActivityType, activityDto.UserId);

        var deviceInfo = ParseUserAgent(userAgent);

        // Parse enum values from strings
        if (!Enum.TryParse<ActivityType>(activityDto.ActivityType, true, out var activityType))
        {
            _logger.LogWarning("Invalid ActivityType: {ActivityType}", activityDto.ActivityType);
            throw new ArgumentException($"Invalid ActivityType: {activityDto.ActivityType}", nameof(activityDto));
        }

        if (!Enum.TryParse<EntityType>(activityDto.EntityType, true, out var entityType))
        {
            _logger.LogWarning("Invalid EntityType: {EntityType}", activityDto.EntityType);
            throw new ArgumentException($"Invalid EntityType: {activityDto.EntityType}", nameof(activityDto));
        }

        DeviceType deviceType = DeviceType.Other;
        if (!string.IsNullOrEmpty(deviceInfo.DeviceType))
        {
            if (!Enum.TryParse<DeviceType>(deviceInfo.DeviceType, true, out deviceType))
                deviceType = DeviceType.Other;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var activity = UserActivityLog.Create(
            activityType: activityType,
            entityType: entityType,
            description: activityDto.Description,
            ipAddress: ipAddress,
            userAgent: userAgent,
            userId: activityDto.UserId,
            entityId: activityDto.EntityId,
            deviceType: deviceType,
            browser: deviceInfo.Browser,
            os: deviceInfo.OS,
            metadata: activityDto.Metadata,
            durationMs: activityDto.DurationMs,
            wasSuccessful: activityDto.WasSuccessful,
            errorMessage: activityDto.ErrorMessage
        );

        await _context.Set<UserActivityLog>().AddAsync(activity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Activity logged successfully with ID: {ActivityId}", activity.Id);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<UserActivityLogDto?> GetActivityByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Retrieving activity with ID: {ActivityId}", id);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var activity = await _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (activity == null)
        {
            _logger.LogWarning("Activity not found with ID: {ActivityId}", id);
            return null;
        }

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<UserActivityLogDto>(activity);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<UserActivityLogDto>> GetUserActivitiesAsync(Guid userId, int days = 30, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving activities for user: {UserId} for last {Days} days", userId, days);

        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var activities = await _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.UserId == userId && a.CreatedAt >= startDate)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {Count} activities for user: {UserId}", activities.Count, userId);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<UserActivityLogDto>>(activities);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<UserActivityLogDto>> GetActivitiesAsync(ActivityFilterDto filter, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving filtered activities - Page: {PageNumber}, Size: {PageSize}", filter.PageNumber, filter.PageSize);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        IQueryable<UserActivityLog> query = _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .AsQueryable();

        if (filter.UserId.HasValue)
            query = query.Where(a => a.UserId == filter.UserId.Value);

        if (!string.IsNullOrEmpty(filter.ActivityType) && 
            Enum.TryParse<ActivityType>(filter.ActivityType, true, out var activityType))
            query = query.Where(a => a.ActivityType == activityType);

        if (!string.IsNullOrEmpty(filter.EntityType) && 
            Enum.TryParse<EntityType>(filter.EntityType, true, out var entityType))
            query = query.Where(a => a.EntityType == entityType);

        if (filter.EntityId.HasValue)
            query = query.Where(a => a.EntityId == filter.EntityId.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(a => a.CreatedAt >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(a => a.CreatedAt <= filter.EndDate.Value);

        if (!string.IsNullOrEmpty(filter.IpAddress))
            query = query.Where(a => a.IpAddress == filter.IpAddress);

        if (!string.IsNullOrEmpty(filter.DeviceType) && 
            Enum.TryParse<DeviceType>(filter.DeviceType, true, out var deviceType))
            query = query.Where(a => a.DeviceType == deviceType);

        if (filter.WasSuccessful.HasValue)
            query = query.Where(a => a.WasSuccessful == filter.WasSuccessful.Value);

        var activities = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Retrieved {Count} filtered activities", activities.Count);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<UserActivityLogDto>>(activities);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<ActivityStatsDto> GetActivityStatsAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating activity statistics for last {Days} days", days);

        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        IQueryable<UserActivityLog> query = _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= startDate);

        var totalActivities = await query.CountAsync(cancellationToken);
        var uniqueUsers = await query
            .Where(a => a.UserId.HasValue)
            .Select(a => a.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de grouping yap
        var activitiesByType = await query
            .GroupBy(a => a.ActivityType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type.ToString(), x => x.Count, cancellationToken);

        var activitiesByDevice = await query
            .GroupBy(a => a.DeviceType)
            .Select(g => new { Device = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Device.ToString(), x => x.Count, cancellationToken);

        var activitiesByHour = await query
            .GroupBy(a => a.CreatedAt.Hour)
            .Select(g => new { Hour = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Hour, x => x.Count, cancellationToken);

        // ✅ PERFORMANCE: Database'de grouping ve ordering yap
        // ✅ userIds'i önce database'de oluştur, sonra topUsersData'yı al
        var userIds = await query
            .Where(a => a.UserId.HasValue)
            .GroupBy(a => a.UserId)
            .Select(g => new
            {
                UserId = g.Key!.Value,
                ActivityCount = g.Count()
            })
            .OrderByDescending(u => u.ActivityCount)
            .Take(10)
            .Select(u => u.UserId)
            .ToListAsync(cancellationToken);
        
        // ✅ PERFORMANCE: Batch load user emails
        var userEmails = await _context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Email, cancellationToken);

        // ✅ PERFORMANCE: Database'de topUsersData'yı al (userIds ile filtrele)
        // ✅ DTO'ları database query'sinde oluştur, memory'de Select YASAK
        var topUsersData = await query
            .Where(a => a.UserId.HasValue && userIds.Contains(a.UserId.Value))
            .GroupBy(a => a.UserId)
            .Select(g => new TopUserActivityDto
            {
                UserId = g.Key!.Value,
                UserEmail = string.Empty, // Will be populated below
                ActivityCount = g.Count(),
                LastActivity = g.Max(a => a.CreatedAt)
            })
            .OrderByDescending(u => u.ActivityCount)
            .Take(10)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: UserEmail'i batch loaded dictionary'den set et (minimal memory işlemi)
        foreach (var user in topUsersData)
        {
            if (userEmails.TryGetValue(user.UserId, out var email) && !string.IsNullOrEmpty(email))
            {
                user.UserEmail = email;
            }
        }
        
        var topUsers = topUsersData;

        var mostViewedProducts = await GetMostViewedProductsAsync(days, 10, cancellationToken);

        // ✅ PERFORMANCE: Database'de average hesapla
        var avgSessionDuration = await query
            .Where(a => a.DurationMs > 0)
            .AverageAsync(a => (decimal?)a.DurationMs, cancellationToken) ?? 0;

        _logger.LogInformation("Activity stats generated - Total: {Total}, Unique Users: {Users}", totalActivities, uniqueUsers);

        return new ActivityStatsDto
        {
            TotalActivities = totalActivities,
            UniqueUsers = uniqueUsers,
            ActivitiesByType = activitiesByType,
            ActivitiesByDevice = activitiesByDevice,
            ActivitiesByHour = activitiesByHour,
            TopUsers = topUsers,
            MostViewedProducts = mostViewedProducts, // ✅ GetMostViewedProductsAsync zaten List döndürüyor
            AverageSessionDuration = avgSessionDuration
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(Guid userId, int days = 7, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving user sessions for user: {UserId} for last {Days} days", userId, days);

        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var activities = await _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.UserId == userId && a.CreatedAt >= startDate)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        var sessions = new List<UserSessionDto>(activities.Count > 0 ? activities.Count / 10 : 1);
        var currentSession = new List<UserActivityLog>();
        var sessionTimeout = TimeSpan.FromMinutes(30);

        foreach (var activity in activities)
        {
            if (currentSession.Any() &&
                (activity.CreatedAt - currentSession.Last().CreatedAt) > sessionTimeout)
            {
                // Start new session
                sessions.Add(CreateSessionDto(currentSession));
                currentSession = new List<UserActivityLog>();
            }

            currentSession.Add(activity);
        }

        // Add final session
        if (currentSession.Any())
        {
            sessions.Add(CreateSessionDto(currentSession));
        }

        _logger.LogInformation("Found {Count} sessions for user: {UserId}", sessions.Count, userId);

        return sessions;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<List<PopularProductDto>> GetMostViewedProductsAsync(int days = 30, int topN = 10, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving most viewed products for last {Days} days, top {TopN}", days, topN);

        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: productIds'i önce database'de oluştur, sonra productActivities'ı al
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !a.IsDeleted (Global Query Filter)
        var productIds = await _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= startDate &&
                       a.EntityType == EntityType.Product &&
                       a.EntityId.HasValue &&
                       (a.ActivityType == ActivityType.ViewProduct ||
                        a.ActivityType == ActivityType.AddToCart))
            .GroupBy(a => a.EntityId)
            .Select(g => new
            {
                ProductId = g.Key!.Value,
                ViewCount = g.Count(a => a.ActivityType == ActivityType.ViewProduct)
            })
            .OrderByDescending(p => p.ViewCount)
            .Take(topN)
            .Select(p => p.ProductId)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de productActivities'ı al (productIds ile filtrele)
        var productActivitiesData = await _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= startDate &&
                       a.EntityType == EntityType.Product &&
                       a.EntityId.HasValue &&
                       productIds.Contains(a.EntityId.Value) &&
                       (a.ActivityType == ActivityType.ViewProduct ||
                        a.ActivityType == ActivityType.AddToCart))
            .GroupBy(a => a.EntityId)
            .Select(g => new
            {
                ProductId = g.Key!.Value,
                ViewCount = g.Count(a => a.ActivityType == ActivityType.ViewProduct),
                AddToCartCount = g.Count(a => a.ActivityType == ActivityType.AddToCart)
            })
            .OrderByDescending(p => p.ViewCount)
            .Take(topN)
            .ToListAsync(cancellationToken);

        var products = await _context.Set<ProductEntity>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);

        var purchases = await _context.Set<OrderItem>()
            .AsNoTracking()
            .Where(oi => productIds.Contains(oi.ProductId) &&
                        oi.Order.CreatedAt >= startDate)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new { ProductId = g.Key, PurchaseCount = g.Sum(oi => oi.Quantity) })
            .ToDictionaryAsync(p => p.ProductId, p => p.PurchaseCount, cancellationToken);

        // ✅ PERFORMANCE: DTO'ları oluştur (minimal memory işlemi - property assignment ve dictionary lookup)
        // Note: Dictionary lookup ve matematiksel işlemler minimal memory işlemleridir
        var result = new List<PopularProductDto>(productActivitiesData.Count);
        foreach (var p in productActivitiesData)
        {
            var purchaseCount = purchases.ContainsKey(p.ProductId) ? purchases[p.ProductId] : 0;
            var conversionRate = p.ViewCount > 0
                ? (decimal)purchaseCount / p.ViewCount * 100
                : 0;
            
            result.Add(new PopularProductDto
            {
                ProductId = p.ProductId,
                ProductName = products.ContainsKey(p.ProductId) ? products[p.ProductId] : "Unknown",
                ViewCount = p.ViewCount,
                AddToCartCount = p.AddToCartCount,
                PurchaseCount = purchaseCount,
                ConversionRate = conversionRate
            });
        }
        
        return result;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task DeleteOldActivitiesAsync(int daysToKeep = 90, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting old activities older than {Days} days", daysToKeep);

        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

        // ✅ PERFORMANCE: Removed manual !a.IsDeleted (Global Query Filter)
        var oldActivities = await _context.Set<UserActivityLog>()
            .Where(a => a.CreatedAt < cutoffDate)
            .ToListAsync(cancellationToken);

        _context.Set<UserActivityLog>().RemoveRange(oldActivities);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogWarning("Deleted {Count} old activity records", oldActivities.Count);
    }


    private UserSessionDto CreateSessionDto(List<UserActivityLog> activities)
    {
        var first = activities.First();
        var last = activities.Last();

        return new UserSessionDto
        {
            UserId = first.UserId,
            UserEmail = first.User?.Email ?? "Anonymous",
            SessionStart = first.CreatedAt,
            SessionEnd = last.CreatedAt,
            DurationMinutes = (int)(last.CreatedAt - first.CreatedAt).TotalMinutes,
            ActivitiesCount = activities.Count,
            // ✅ ARCHITECTURE: AutoMapper kullan
            Activities = _mapper.Map<List<UserActivityLogDto>>(activities)
        };
    }

    private (string DeviceType, string Browser, string OS) ParseUserAgent(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent))
        {
            return ("Unknown", "Unknown", "Unknown");
        }

        var deviceType = "Desktop";
        if (userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase))
            deviceType = "Mobile";
        else if (userAgent.Contains("Tablet", StringComparison.OrdinalIgnoreCase))
            deviceType = "Tablet";

        var browser = "Unknown";
        if (userAgent.Contains("Chrome", StringComparison.OrdinalIgnoreCase))
            browser = "Chrome";
        else if (userAgent.Contains("Firefox", StringComparison.OrdinalIgnoreCase))
            browser = "Firefox";
        else if (userAgent.Contains("Safari", StringComparison.OrdinalIgnoreCase))
            browser = "Safari";
        else if (userAgent.Contains("Edge", StringComparison.OrdinalIgnoreCase))
            browser = "Edge";

        var os = "Unknown";
        if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase))
            os = "Windows";
        else if (userAgent.Contains("Mac", StringComparison.OrdinalIgnoreCase))
            os = "macOS";
        else if (userAgent.Contains("Linux", StringComparison.OrdinalIgnoreCase))
            os = "Linux";
        else if (userAgent.Contains("Android", StringComparison.OrdinalIgnoreCase))
            os = "Android";
        else if (userAgent.Contains("iOS", StringComparison.OrdinalIgnoreCase) ||
                 userAgent.Contains("iPhone", StringComparison.OrdinalIgnoreCase))
            os = "iOS";

        return (deviceType, browser, os);
    }
}
