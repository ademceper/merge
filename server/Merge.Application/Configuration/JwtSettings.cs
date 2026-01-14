namespace Merge.Application.Configuration;

/// <summary>
/// JWT token işlemleri için configuration ayarları
/// BOLUM 12.1: Magic Number Sorunu - Tüm magic number'lar configuration'a taşındı
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// JWT secret key (en az 32 karakter olmalı)
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// JWT issuer
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// JWT audience
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Access token geçerlilik süresi (dakika)
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh token geçerlilik süresi (gün)
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// Clock skew tolerance (saniye) - Token validation için zaman farkı toleransı
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 0;
}

