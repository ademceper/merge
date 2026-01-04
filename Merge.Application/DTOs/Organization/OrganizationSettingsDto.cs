using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Organization;

/// <summary>
/// Organization ayarlari icin typed DTO - Dictionary yerine guvenli
/// </summary>
public class OrganizationSettingsDto
{
    /// <summary>
    /// Organizasyon aktif mi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Coklu magaza destegi
    /// </summary>
    public bool MultiStoreEnabled { get; set; } = false;

    /// <summary>
    /// Maksimum magaza sayisi
    /// </summary>
    [Range(1, 1000)]
    public int MaxStores { get; set; } = 10;

    /// <summary>
    /// Maksimum kullanici sayisi
    /// </summary>
    [Range(1, 10000)]
    public int MaxUsers { get; set; } = 50;

    /// <summary>
    /// API erisimi aktif mi
    /// </summary>
    public bool ApiAccessEnabled { get; set; } = false;

    /// <summary>
    /// Varsayilan dil kodu
    /// </summary>
    [StringLength(10)]
    public string? DefaultLanguage { get; set; }

    /// <summary>
    /// Varsayilan para birimi
    /// </summary>
    [StringLength(3)]
    public string? DefaultCurrency { get; set; }

    /// <summary>
    /// Varsayilan zaman dilimi
    /// </summary>
    [StringLength(50)]
    public string? DefaultTimezone { get; set; }

    /// <summary>
    /// 2FA zorunlu mu
    /// </summary>
    public bool Require2FA { get; set; } = false;

    /// <summary>
    /// SSO aktif mi
    /// </summary>
    public bool SsoEnabled { get; set; } = false;

    /// <summary>
    /// SSO saglayici
    /// </summary>
    [StringLength(50)]
    public string? SsoProvider { get; set; }

    /// <summary>
    /// IP whitelist aktif mi
    /// </summary>
    public bool IpWhitelistEnabled { get; set; } = false;

    /// <summary>
    /// Izin verilen IP adresleri (virgul ile ayrilmis)
    /// </summary>
    [StringLength(2000)]
    public string? AllowedIpAddresses { get; set; }
}
