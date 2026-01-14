using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Notifications;

namespace Merge.Application.DTOs.Notification;

/// <summary>
/// Create Notification DTO - BOLUM 7.1.5: Records (C# 12 modern features)
/// </summary>
public record CreateNotificationDto(
    [Required] Guid UserId,
    [Required] NotificationType Type,
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Başlık en az 2, en fazla 200 karakter olmalıdır.")]
    string Title,
    [Required]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Mesaj en az 1, en fazla 2000 karakter olmalıdır.")]
    string Message,
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    string? Link = null,
    string? Data = null);
