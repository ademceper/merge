using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.DTOs.Organization;

/// <summary>
/// Team ayarlari icin typed DTO - Dictionary yerine guvenli
/// </summary>
public record TeamSettingsDto(
    /// <summary>
    /// Takim aktif mi
    /// </summary>
    bool IsActive = true,
    /// <summary>
    /// Maksimum uye sayisi
    /// </summary>
    [Range(1, 1000)] int MaxMembers = 50,
    /// <summary>
    /// Urun yonetimi izni
    /// </summary>
    bool CanManageProducts = false,
    /// <summary>
    /// Siparis yonetimi izni
    /// </summary>
    bool CanManageOrders = false,
    /// <summary>
    /// Musteri yonetimi izni
    /// </summary>
    bool CanManageCustomers = false,
    /// <summary>
    /// Finans yonetimi izni
    /// </summary>
    bool CanManageFinance = false,
    /// <summary>
    /// Rapor gorunturleme izni
    /// </summary>
    bool CanViewReports = false,
    /// <summary>
    /// Ayar degistirme izni
    /// </summary>
    bool CanChangeSettings = false,
    /// <summary>
    /// Varsayilan bildirim tercihleri
    /// </summary>
    bool EmailNotificationsEnabled = true,
    /// <summary>
    /// Varsayilan dil
    /// </summary>
    [StringLength(10)] string? DefaultLanguage = null);
