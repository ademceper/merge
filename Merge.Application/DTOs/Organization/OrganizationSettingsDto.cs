using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Organization;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
/// <summary>
/// Organization ayarlari icin typed DTO - Dictionary yerine guvenli
/// </summary>
public record OrganizationSettingsDto(
    /// <summary>
    /// Organizasyon aktif mi
    /// </summary>
    bool IsActive = true,
    /// <summary>
    /// Coklu magaza destegi
    /// </summary>
    bool MultiStoreEnabled = false,
    /// <summary>
    /// Maksimum magaza sayisi
    /// </summary>
    [Range(1, 1000)] int MaxStores = 10,
    /// <summary>
    /// Maksimum kullanici sayisi
    /// </summary>
    [Range(1, 10000)] int MaxUsers = 50,
    /// <summary>
    /// API erisimi aktif mi
    /// </summary>
    bool ApiAccessEnabled = false,
    /// <summary>
    /// Varsayilan dil kodu
    /// </summary>
    [StringLength(10)] string? DefaultLanguage = null,
    /// <summary>
    /// Varsayilan para birimi
    /// </summary>
    [StringLength(3)] string? DefaultCurrency = null,
    /// <summary>
    /// Varsayilan zaman dilimi
    /// </summary>
    [StringLength(50)] string? DefaultTimezone = null,
    /// <summary>
    /// 2FA zorunlu mu
    /// </summary>
    bool Require2FA = false,
    /// <summary>
    /// SSO aktif mi
    /// </summary>
    bool SsoEnabled = false,
    /// <summary>
    /// SSO saglayici
    /// </summary>
    [StringLength(50)] string? SsoProvider = null,
    /// <summary>
    /// IP whitelist aktif mi
    /// </summary>
    bool IpWhitelistEnabled = false,
    /// <summary>
    /// Izin verilen IP adresleri (virgul ile ayrilmis)
    /// </summary>
    [StringLength(2000)] string? AllowedIpAddresses = null);
