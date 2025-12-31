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
    private readonly ILogger<UserActivityService> _logger;

    public UserActivityService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UserActivityService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
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

        return MapToDto(activity);
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

        return activities.Select(MapToDto);
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

        return activities.Select(MapToDto);
    }

    public async Task<ActivityStatsDto> GetActivityStatsAsync(int days = 30)
    {
        _logger.LogInformation("Generating activity statistics for last {Days} days", days);

        var startDate = DateTime.UtcNow.AddDays(-days);

        var activities = await _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= startDate)
            .ToListAsync();

        var totalActivities = activities.Count;
        var uniqueUsers = activities.Where(a => a.UserId.HasValue).Select(a => a.UserId).Distinct().Count();

        var activitiesByType = activities
            .GroupBy(a => a.ActivityType)
            .ToDictionary(g => g.Key, g => g.Count());

        var activitiesByDevice = activities
            .GroupBy(a => a.DeviceType)
            .ToDictionary(g => g.Key, g => g.Count());

        var activitiesByHour = activities
            .GroupBy(a => a.CreatedAt.Hour)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var topUsersData = await _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Where(a => a.CreatedAt >= startDate && a.UserId.HasValue)
            .Include(a => a.User)
            .GroupBy(a => a.UserId)
            .ToListAsync();

        var topUsers = topUsersData
            .Select(g => new TopUserActivityDto
            {
                UserId = g.Key!.Value,
                UserEmail = g.FirstOrDefault()?.User?.Email ?? string.Empty,
                ActivityCount = g.Count(),
                LastActivity = g.Max(a => a.CreatedAt)
            })
            .OrderByDescending(u => u.ActivityCount)
            .Take(10)
            .ToList();

        var mostViewedProducts = await GetMostViewedProductsAsync(days, 10);

        var avgSessionDuration = activities
            .Where(a => a.DurationMs > 0)
            .Average(a => (decimal?)a.DurationMs) ?? 0;

        _logger.LogInformation("Activity stats generated - Total: {Total}, Unique Users: {Users}", totalActivities, uniqueUsers);

        return new ActivityStatsDto
        {
            TotalActivities = totalActivities,
            UniqueUsers = uniqueUsers,
            ActivitiesByType = activitiesByType,
            ActivitiesByDevice = activitiesByDevice,
            ActivitiesByHour = activitiesByHour,
            TopUsers = topUsers.ToList(),
            MostViewedProducts = mostViewedProducts.ToList(),
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

    public async Task<IEnumerable<PopularProductDto>> GetMostViewedProductsAsync(int days = 30, int topN = 10)
    {
        _logger.LogInformation("Retrieving most viewed products for last {Days} days, top {TopN}", days, topN);

        var startDate = DateTime.UtcNow.AddDays(-days);

        var productActivities = await _context.Set<UserActivityLog>()
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
                ViewCount = g.Count(a => a.ActivityType == "ViewProduct"),
                AddToCartCount = g.Count(a => a.ActivityType == "AddToCart")
            })
            .OrderByDescending(p => p.ViewCount)
            .Take(topN)
            .ToListAsync();

        var productIds = productActivities.Select(p => p.ProductId).ToList();

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

        return productActivities.Select(p => new PopularProductDto
        {
            ProductId = p.ProductId,
            ProductName = products.ContainsKey(p.ProductId) ? products[p.ProductId] : "Unknown",
            ViewCount = p.ViewCount,
            AddToCartCount = p.AddToCartCount,
            PurchaseCount = purchases.ContainsKey(p.ProductId) ? purchases[p.ProductId] : 0,
            ConversionRate = p.ViewCount > 0
                ? (purchases.ContainsKey(p.ProductId) ? (decimal)purchases[p.ProductId] / p.ViewCount * 100 : 0)
                : 0
        }).ToList();
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

    private UserActivityLogDto MapToDto(UserActivityLog activity)
    {
        return new UserActivityLogDto
        {
            Id = activity.Id,
            UserId = activity.UserId,
            UserEmail = activity.User?.Email ?? "Anonymous",
            ActivityType = activity.ActivityType,
            EntityType = activity.EntityType,
            EntityId = activity.EntityId,
            Description = activity.Description,
            IpAddress = activity.IpAddress,
            UserAgent = activity.UserAgent,
            DeviceType = activity.DeviceType,
            Browser = activity.Browser,
            OS = activity.OS,
            Location = activity.Location,
            CreatedAt = activity.CreatedAt,
            DurationMs = activity.DurationMs,
            WasSuccessful = activity.WasSuccessful,
            ErrorMessage = activity.ErrorMessage
        };
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
            Activities = activities.Select(MapToDto).ToList()
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
