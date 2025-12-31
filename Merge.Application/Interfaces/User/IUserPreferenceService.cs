using Merge.Application.DTOs.User;

namespace Merge.Application.Interfaces.User;

public interface IUserPreferenceService
{
    Task<UserPreferenceDto> GetUserPreferencesAsync(Guid userId);
    Task<UserPreferenceDto> UpdateUserPreferencesAsync(Guid userId, UpdateUserPreferenceDto dto);
    Task<UserPreferenceDto> ResetToDefaultsAsync(Guid userId);
}
