using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.DTOs.Notification;

/// <summary>
/// Bulk Update Notification Preferences DTO - BOLUM 7.1.5: Records (C# 12 modern features)
/// </summary>
public record BulkUpdateNotificationPreferencesDto(
    [Required]
    [MinLength(1, ErrorMessage = "En az bir tercih belirtilmelidir.")]
    List<CreateNotificationPreferenceDto> Preferences);
