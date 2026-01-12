using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.User;

public interface IUserPreferenceService
{
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    Task<UserPreferenceDto> GetUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserPreferenceDto> UpdateUserPreferencesAsync(Guid userId, UpdateUserPreferenceDto dto, CancellationToken cancellationToken = default);
    Task<UserPreferenceDto> ResetToDefaultsAsync(Guid userId, CancellationToken cancellationToken = default);
}
