using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Notification;

/// <summary>
/// Notification template ayarlari icin typed DTO - Dictionary yerine guvenli
/// </summary>
public class NotificationTemplateSettingsDto
{
    /// <summary>
    /// Template aktif mi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Varsayilan dil
    /// </summary>
    [StringLength(10)]
    public string? DefaultLanguage { get; set; } = "tr";

    /// <summary>
    /// Konu satiri
    /// </summary>
    [StringLength(200)]
    public string? DefaultSubject { get; set; }

    /// <summary>
    /// Gonderen adi
    /// </summary>
    [StringLength(100)]
    public string? SenderName { get; set; }

    /// <summary>
    /// Gonderen email
    /// </summary>
    [StringLength(200)]
    [EmailAddress]
    public string? SenderEmail { get; set; }

    /// <summary>
    /// Reply-to email
    /// </summary>
    [StringLength(200)]
    [EmailAddress]
    public string? ReplyToEmail { get; set; }

    /// <summary>
    /// HTML format kullan
    /// </summary>
    public bool UseHtmlFormat { get; set; } = true;

    /// <summary>
    /// Tracking aktif mi
    /// </summary>
    public bool TrackingEnabled { get; set; } = false;

    /// <summary>
    /// Retry sayisi
    /// </summary>
    [Range(0, 10)]
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Retry araligi (dakika)
    /// </summary>
    [Range(1, 1440)]
    public int RetryIntervalMinutes { get; set; } = 5;
}

/// <summary>
/// Notification template degiskenleri icin typed DTO
/// </summary>
public class NotificationVariablesDto
{
    [StringLength(100)]
    public string? CustomerName { get; set; }

    [StringLength(200)]
    public string? CustomerEmail { get; set; }

    [StringLength(50)]
    public string? OrderNumber { get; set; }

    [StringLength(500)]
    public string? ActionUrl { get; set; }

    [StringLength(100)]
    public string? CompanyName { get; set; }

    [StringLength(500)]
    public string? LogoUrl { get; set; }

    public decimal? Amount { get; set; }

    [StringLength(3)]
    public string? Currency { get; set; }

    public DateTime? ExpirationDate { get; set; }

    [StringLength(100)]
    public string? ProductName { get; set; }

    [StringLength(1000)]
    public string? CustomMessage { get; set; }
}
