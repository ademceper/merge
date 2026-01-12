using Merge.Domain.Enums;
using Merge.Domain.Modules.Identity;
namespace Merge.Application.DTOs.User;

// ✅ BOLUM 4.2: Record DTOs (ZORUNLU) - Immutability için record kullan
public record UserActivityLogDto(
    Guid Id,
    Guid? UserId,
    string UserEmail,
    string ActivityType,
    string EntityType,
    Guid? EntityId,
    string Description,
    string IpAddress,
    string UserAgent,
    string DeviceType,
    string Browser,
    string OS,
    string Location,
    DateTime CreatedAt,
    int DurationMs,
    bool WasSuccessful,
    string? ErrorMessage);
