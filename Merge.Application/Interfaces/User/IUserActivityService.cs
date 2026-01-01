using Merge.Application.DTOs.User;
namespace Merge.Application.Interfaces.User;

public interface IUserActivityService
{
    Task LogActivityAsync(CreateActivityLogDto activityDto, string ipAddress, string userAgent);
    Task<UserActivityLogDto?> GetActivityByIdAsync(Guid id);
    Task<IEnumerable<UserActivityLogDto>> GetUserActivitiesAsync(Guid userId, int days = 30);
    Task<IEnumerable<UserActivityLogDto>> GetActivitiesAsync(ActivityFilterDto filter);
    Task<ActivityStatsDto> GetActivityStatsAsync(int days = 30);
    Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(Guid userId, int days = 7);
    Task<List<PopularProductDto>> GetMostViewedProductsAsync(int days = 30, int topN = 10);
    Task DeleteOldActivitiesAsync(int daysToKeep = 90);
}
