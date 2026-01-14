using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Payment;

namespace Merge.Application.DTOs.Notification;

/// <summary>
/// Notification template ayarlari icin typed DTO - Dictionary yerine guvenli
/// BOLUM 7.1.5: Records (C# 12 modern features) - Opsiyonel ama tutarlılık için record kullanıyoruz
/// </summary>
public record NotificationTemplateSettingsDto(
    bool IsActive = true,
    [StringLength(10)] string? DefaultLanguage = "tr",
    [StringLength(200)] string? DefaultSubject = null,
    [StringLength(100)] string? SenderName = null,
    [StringLength(200)] [EmailAddress] string? SenderEmail = null,
    [StringLength(200)] [EmailAddress] string? ReplyToEmail = null,
    bool UseHtmlFormat = true,
    bool TrackingEnabled = false,
    [Range(0, 10)] int MaxRetries = 3,
    [Range(1, 1440)] int RetryIntervalMinutes = 5);

/// <summary>
/// Notification template degiskenleri icin typed DTO
/// BOLUM 7.1.5: Records (C# 12 modern features) - Opsiyonel ama tutarlılık için record kullanıyoruz
/// </summary>
public record NotificationVariablesDto(
    [StringLength(100)] string? CustomerName = null,
    [StringLength(200)] string? CustomerEmail = null,
    [StringLength(50)] string? OrderNumber = null,
    [StringLength(500)] string? ActionUrl = null,
    [StringLength(100)] string? CompanyName = null,
    [StringLength(500)] string? LogoUrl = null,
    decimal? Amount = null,
    [StringLength(3)] string? Currency = null,
    DateTime? ExpirationDate = null,
    [StringLength(100)] string? ProductName = null,
    [StringLength(1000)] string? CustomMessage = null);
