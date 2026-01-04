using Merge.Application.DTOs.User;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.User;

public interface IUserActivityService
{
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    Task LogActivityAsync(CreateActivityLogDto activityDto, string ipAddress, string userAgent, CancellationToken cancellationToken = default);
    Task<UserActivityLogDto?> GetActivityByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserActivityLogDto>> GetUserActivitiesAsync(Guid userId, int days = 30, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserActivityLogDto>> GetActivitiesAsync(ActivityFilterDto filter, CancellationToken cancellationToken = default);
    Task<ActivityStatsDto> GetActivityStatsAsync(int days = 30, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(Guid userId, int days = 7, CancellationToken cancellationToken = default);
    Task<List<PopularProductDto>> GetMostViewedProductsAsync(int days = 30, int topN = 10, CancellationToken cancellationToken = default);
    Task DeleteOldActivitiesAsync(int daysToKeep = 90, CancellationToken cancellationToken = default);
}
