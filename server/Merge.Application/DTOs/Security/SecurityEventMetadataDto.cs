using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.Security;

/// <summary>
/// Security event metadata icin typed DTO - Dictionary yerine guvenli
/// </summary>
public class SecurityEventMetadataDto
{
    /// <summary>
    /// IP adresi
    /// </summary>
    [StringLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent
    /// </summary>
    [StringLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Cihaz tipi
    /// </summary>
    [StringLength(50)]
    public string? DeviceType { get; set; }

    /// <summary>
    /// Isletim sistemi
    /// </summary>
    [StringLength(100)]
    public string? OperatingSystem { get; set; }

    /// <summary>
    /// Tarayici
    /// </summary>
    [StringLength(100)]
    public string? Browser { get; set; }

    /// <summary>
    /// Konum (ulke)
    /// </summary>
    [StringLength(100)]
    public string? Country { get; set; }

    /// <summary>
    /// Konum (sehir)
    /// </summary>
    [StringLength(100)]
    public string? City { get; set; }

    /// <summary>
    /// Session ID
    /// </summary>
    [StringLength(100)]
    public string? SessionId { get; set; }

    /// <summary>
    /// Request ID
    /// </summary>
    [StringLength(100)]
    public string? RequestId { get; set; }

    /// <summary>
    /// Endpoint
    /// </summary>
    [StringLength(500)]
    public string? Endpoint { get; set; }

    /// <summary>
    /// HTTP metodu
    /// </summary>
    [StringLength(10)]
    public string? HttpMethod { get; set; }

    /// <summary>
    /// Risk skoru
    /// </summary>
    [Range(0, 100)]
    public int? RiskScore { get; set; }

    /// <summary>
    /// Olay zamanı
    /// </summary>
    public DateTime? EventTime { get; set; }

    /// <summary>
    /// Basarisiz deneme sayisi
    /// </summary>
    [Range(0, 1000)]
    public int? FailedAttempts { get; set; }

    /// <summary>
    /// Onceki olay ID
    /// </summary>
    public Guid? PreviousEventId { get; set; }
}

/// <summary>
/// Fraud detection metadata icin typed DTO - Dictionary yerine guvenli
/// </summary>
public class FraudDetectionMetadataDto
{
    /// <summary>
    /// Risk skoru
    /// </summary>
    [Range(0, 100)]
    public decimal? RiskScore { get; set; }

    /// <summary>
    /// Risk seviyesi
    /// </summary>
    [StringLength(20)]
    public string? RiskLevel { get; set; }

    /// <summary>
    /// Tetiklenen kurallar (virgul ile ayrilmis)
    /// </summary>
    [StringLength(1000)]
    public string? TriggeredRules { get; set; }

    /// <summary>
    /// IP risk skoru
    /// </summary>
    [Range(0, 100)]
    public decimal? IpRiskScore { get; set; }

    /// <summary>
    /// Email risk skoru
    /// </summary>
    [Range(0, 100)]
    public decimal? EmailRiskScore { get; set; }

    /// <summary>
    /// Cihaz risk skoru
    /// </summary>
    [Range(0, 100)]
    public decimal? DeviceRiskScore { get; set; }

    /// <summary>
    /// Velocity check sonucu
    /// </summary>
    public bool? VelocityCheckPassed { get; set; }

    /// <summary>
    /// Adres dogrulama sonucu
    /// </summary>
    public bool? AddressVerified { get; set; }

    /// <summary>
    /// CVV dogrulama sonucu
    /// </summary>
    public bool? CvvVerified { get; set; }

    /// <summary>
    /// 3DS sonucu
    /// </summary>
    [StringLength(50)]
    public string? ThreeDsResult { get; set; }

    /// <summary>
    /// Karar
    /// </summary>
    [StringLength(50)]
    public string? Decision { get; set; }

    /// <summary>
    /// Karar nedeni
    /// </summary>
    [StringLength(500)]
    public string? DecisionReason { get; set; }
}
