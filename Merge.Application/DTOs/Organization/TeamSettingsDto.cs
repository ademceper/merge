using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Organization;

/// <summary>
/// Team ayarlari icin typed DTO - Dictionary yerine guvenli
/// </summary>
public class TeamSettingsDto
{
    /// <summary>
    /// Takim aktif mi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Maksimum uye sayisi
    /// </summary>
    [Range(1, 1000)]
    public int MaxMembers { get; set; } = 50;

    /// <summary>
    /// Urun yonetimi izni
    /// </summary>
    public bool CanManageProducts { get; set; } = false;

    /// <summary>
    /// Siparis yonetimi izni
    /// </summary>
    public bool CanManageOrders { get; set; } = false;

    /// <summary>
    /// Musteri yonetimi izni
    /// </summary>
    public bool CanManageCustomers { get; set; } = false;

    /// <summary>
    /// Finans yonetimi izni
    /// </summary>
    public bool CanManageFinance { get; set; } = false;

    /// <summary>
    /// Rapor gorunturleme izni
    /// </summary>
    public bool CanViewReports { get; set; } = false;

    /// <summary>
    /// Ayar degistirme izni
    /// </summary>
    public bool CanChangeSettings { get; set; } = false;

    /// <summary>
    /// Varsayilan bildirim tercihleri
    /// </summary>
    public bool EmailNotificationsEnabled { get; set; } = true;

    /// <summary>
    /// Varsayilan dil
    /// </summary>
    [StringLength(10)]
    public string? DefaultLanguage { get; set; }
}
