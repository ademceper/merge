using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserEntity = Merge.Domain.Entities.User;
using Merge.Application.Interfaces.User;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Application.DTOs.User;
using Merge.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

namespace Merge.Application.Services.User;

public class UserActivityService : IUserActivityService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UserActivityService> _logger;

    public UserActivityService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UserActivityService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task LogActivityAsync(CreateActivityLogDto activityDto, string ipAddress, string userAgent)
    {
        _logger.LogDebug("Logging activity: {ActivityType} for user: {UserId}", activityDto.ActivityType, activityDto.UserId);

        var deviceInfo = ParseUserAgent(userAgent);

        var activity = new UserActivityLog
        {
            UserId = activityDto.UserId,
            ActivityType = activityDto.ActivityType,
            EntityType = activityDto.EntityType,
            EntityId = activityDto.EntityId,
            Description = activityDto.Description,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            DeviceType = deviceInfo.DeviceType,
            Browser = deviceInfo.Browser,
            OS = deviceInfo.OS,
            Metadata = activityDto.Metadata ?? string.Empty,
            DurationMs = activityDto.DurationMs,
            WasSuccessful = activityDto.WasSuccessful,
            ErrorMessage = activityDto.ErrorMessage
        };

        await _context.Set<UserActivityLog>().AddAsync(activity);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogDebug("Activity logged successfully with ID: {ActivityId}", activity.Id);
    }

    public async Task<UserActivityLogDto?> GetActivityByIdAsync(Guid id)
    {
        _logger.LogDebug("Retrieving activity with ID: {ActivityId}", id);

        var activity = await _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (activity == null)
        {
            _logger.LogWarning("Activity not found with ID: {ActivityId}", id);
            return null;
        }

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<UserActivityLogDto>(activity);
    }

    public async Task<IEnumerable<UserActivityLogDto>> GetUserActivitiesAsync(Guid userId, int days = 30)
    {
        _logger.LogInformation("Retrieving activities for user: {UserId} for last {Days} days", userId, days);

        var startDate = DateTime.UtcNow.AddDays(-days);

        var activities = await _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.UserId == userId && a.CreatedAt >= startDate)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        _logger.LogInformation("Found {Count} activities for user: {UserId}", activities.Count, userId);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<UserActivityLogDto>>(activities);
    }

    public async Task<IEnumerable<UserActivityLogDto>> GetActivitiesAsync(ActivityFilterDto filter)
    {
        _logger.LogInformation("Retrieving filtered activities - Page: {PageNumber}, Size: {PageSize}", filter.PageNumber, filter.PageSize);

        var query = _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .AsQueryable();

        if (filter.UserId.HasValue)
            query = query.Where(a => a.UserId == filter.UserId.Value);

        if (!string.IsNullOrEmpty(filter.ActivityType))
            query = query.Where(a => a.ActivityType == filter.ActivityType);

        if (!string.IsNullOrEmpty(filter.EntityType))
            query = query.Where(a => a.EntityType == filter.EntityType);

        if (filter.EntityId.HasValue)
            query = query.Where(a => a.EntityId == filter.EntityId.Value);

        if (filter.StartDate.HasValue)
            query = query.Where(a => a.CreatedAt >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(a => a.CreatedAt <= filter.EndDate.Value);

        if (!string.IsNullOrEmpty(filter.IpAddress))
            query = query.Where(a => a.IpAddress == filter.IpAddress);

        if (!string.IsNullOrEmpty(filter.DeviceType))
            query = query.Where(a => a.DeviceType == filter.DeviceType);

        if (filter.WasSuccessful.HasValue)
            query = query.Where(a => a.WasSuccessful == filter.WasSuccessful.Value);

        var activities = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} filtered activities", activities.Count);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<UserActivityLogDto>>(activities);
    }

    public async Task<ActivityStatsDto> GetActivityStatsAsync(int days = 30)
    {
        _logger.LogInformation("Generating activity statistics for last {Days} days", days);

        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: Database'de aggregations yap, memory'de işlem YASAK
        var query = _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= startDate);

        var totalActivities = await query.CountAsync();
        var uniqueUsers = await query
            .Where(a => a.UserId.HasValue)
            .Select(a => a.UserId)
            .Distinct()
            .CountAsync();

        // ✅ PERFORMANCE: Database'de grouping yap
        var activitiesByType = await query
            .GroupBy(a => a.ActivityType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);

        var activitiesByDevice = await query
            .GroupBy(a => a.DeviceType)
            .Select(g => new { Device = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Device, x => x.Count);

        var activitiesByHour = await query
            .GroupBy(a => a.CreatedAt.Hour)
            .Select(g => new { Hour = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Hour, x => x.Count);

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
            .ToListAsync();
        
        // ✅ PERFORMANCE: Batch load user emails
        var userEmails = await _context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Email);

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
            .ToListAsync();

        // ✅ PERFORMANCE: UserEmail'i batch loaded dictionary'den set et (minimal memory işlemi)
        foreach (var user in topUsersData)
        {
            if (userEmails.ContainsKey(user.UserId))
            {
                user.UserEmail = userEmails[user.UserId];
            }
        }
        
        var topUsers = topUsersData;

        var mostViewedProducts = await GetMostViewedProductsAsync(days, 10);

        // ✅ PERFORMANCE: Database'de average hesapla
        var avgSessionDuration = await query
            .Where(a => a.DurationMs > 0)
            .AverageAsync(a => (decimal?)a.DurationMs) ?? 0;

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

    public async Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(Guid userId, int days = 7)
    {
        _logger.LogInformation("Retrieving user sessions for user: {UserId} for last {Days} days", userId, days);

        var startDate = DateTime.UtcNow.AddDays(-days);

        var activities = await _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .Where(a => a.UserId == userId && a.CreatedAt >= startDate)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();

        var sessions = new List<UserSessionDto>();
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

    public async Task<List<PopularProductDto>> GetMostViewedProductsAsync(int days = 30, int topN = 10)
    {
        _logger.LogInformation("Retrieving most viewed products for last {Days} days, top {TopN}", days, topN);

        var startDate = DateTime.UtcNow.AddDays(-days);

        // ✅ PERFORMANCE: productIds'i önce database'de oluştur, sonra productActivities'ı al
        var productIds = await _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= startDate &&
                       a.EntityType == "Product" &&
                       a.EntityId.HasValue &&
                       (a.ActivityType == "ViewProduct" ||
                        a.ActivityType == "AddToCart"))
            .GroupBy(a => a.EntityId)
            .Select(g => new
            {
                ProductId = g.Key!.Value,
                ViewCount = g.Count(a => a.ActivityType == "ViewProduct")
            })
            .OrderByDescending(p => p.ViewCount)
            .Take(topN)
            .Select(p => p.ProductId)
            .ToListAsync();

        // ✅ PERFORMANCE: Database'de productActivities'ı al (productIds ile filtrele)
        var productActivitiesData = await _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= startDate &&
                       a.EntityType == "Product" &&
                       a.EntityId.HasValue &&
                       productIds.Contains(a.EntityId.Value) &&
                       (a.ActivityType == "ViewProduct" ||
                        a.ActivityType == "AddToCart"))
            .GroupBy(a => a.EntityId)
            .Select(g => new
            {
                ProductId = g.Key!.Value,
                ViewCount = g.Count(a => a.ActivityType == "ViewProduct"),
                AddToCartCount = g.Count(a => a.ActivityType == "AddToCart")
            })
            .OrderByDescending(p => p.ViewCount)
            .Take(topN)
            .ToListAsync();

        var products = await _context.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name);

        var purchases = await _context.OrderItems
            .AsNoTracking()
            .Where(oi => productIds.Contains(oi.ProductId) &&
                        oi.Order.CreatedAt >= startDate)
            .GroupBy(oi => oi.ProductId)
            .Select(g => new { ProductId = g.Key, PurchaseCount = g.Sum(oi => oi.Quantity) })
            .ToDictionaryAsync(p => p.ProductId, p => p.PurchaseCount);

        // ✅ PERFORMANCE: DTO'ları oluştur (minimal memory işlemi - property assignment ve dictionary lookup)
        // Note: Dictionary lookup ve matematiksel işlemler minimal memory işlemleridir
        var result = new List<PopularProductDto>();
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

    public async Task DeleteOldActivitiesAsync(int daysToKeep = 90)
    {
        _logger.LogInformation("Deleting old activities older than {Days} days", daysToKeep);

        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

        var oldActivities = await _context.Set<UserActivityLog>()
            .Where(a => a.CreatedAt < cutoffDate)
            .ToListAsync();

        _context.Set<UserActivityLog>().RemoveRange(oldActivities);
        await _unitOfWork.SaveChangesAsync();

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
