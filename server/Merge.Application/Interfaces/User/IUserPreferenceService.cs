using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Interfaces.User;

public interface IUserPreferenceService
{
    Task<UserPreferenceDto> GetUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserPreferenceDto> UpdateUserPreferencesAsync(Guid userId, UpdateUserPreferenceDto dto, CancellationToken cancellationToken = default);
    Task<UserPreferenceDto> ResetToDefaultsAsync(Guid userId, CancellationToken cancellationToken = default);
}
