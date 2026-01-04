using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Analytics;

/// <summary>
/// Report ayarlari icin typed DTO - Dictionary yerine guvenli
/// </summary>
public class ReportSettingsDto
{
    /// <summary>
    /// Rapor aktif mi
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Varsayilan tarih araligi (gun)
    /// </summary>
    [Range(1, 365)]
    public int DefaultDateRangeDays { get; set; } = 30;

    /// <summary>
    /// Grafik tipi
    /// </summary>
    [StringLength(50)]
    public string? ChartType { get; set; }

    /// <summary>
    /// Gruplama tipi (gun, hafta, ay)
    /// </summary>
    [StringLength(20)]
    public string? GroupBy { get; set; }

    /// <summary>
    /// Sayfa basina kayit
    /// </summary>
    [Range(10, 1000)]
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// CSV export aktif mi
    /// </summary>
    public bool CsvExportEnabled { get; set; } = true;

    /// <summary>
    /// Excel export aktif mi
    /// </summary>
    public bool ExcelExportEnabled { get; set; } = true;

    /// <summary>
    /// PDF export aktif mi
    /// </summary>
    public bool PdfExportEnabled { get; set; } = false;

    /// <summary>
    /// Email ile rapor gonderimi aktif mi
    /// </summary>
    public bool EmailDeliveryEnabled { get; set; } = false;

    /// <summary>
    /// Rapor email alicilari (virgul ile ayrilmis)
    /// </summary>
    [StringLength(1000)]
    public string? EmailRecipients { get; set; }

    /// <summary>
    /// Zamanlanmis rapor aktif mi
    /// </summary>
    public bool ScheduledEnabled { get; set; } = false;

    /// <summary>
    /// Zamanlama tipi (daily, weekly, monthly)
    /// </summary>
    [StringLength(20)]
    public string? ScheduleType { get; set; }

    /// <summary>
    /// Zamanlama saati
    /// </summary>
    public TimeSpan? ScheduleTime { get; set; }

    /// <summary>
    /// Zamanlama gunu (haftalik icin)
    /// </summary>
    [Range(0, 6)]
    public int? ScheduleDayOfWeek { get; set; }

    /// <summary>
    /// Zamanlama gunu (aylik icin)
    /// </summary>
    [Range(1, 31)]
    public int? ScheduleDayOfMonth { get; set; }
}

/// <summary>
/// Report filtreleri icin typed DTO - Dictionary yerine guvenli
/// </summary>
public class ReportFiltersSettingsDto
{
    /// <summary>
    /// Baslangic tarihi
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Bitis tarihi
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Kategori ID'leri (virgul ile ayrilmis)
    /// </summary>
    [StringLength(1000)]
    public string? CategoryIds { get; set; }

    /// <summary>
    /// Urun ID'leri (virgul ile ayrilmis)
    /// </summary>
    [StringLength(1000)]
    public string? ProductIds { get; set; }

    /// <summary>
    /// Satici ID'leri (virgul ile ayrilmis)
    /// </summary>
    [StringLength(1000)]
    public string? SellerIds { get; set; }

    /// <summary>
    /// Durum filtreleri (virgul ile ayrilmis)
    /// </summary>
    [StringLength(500)]
    public string? Statuses { get; set; }

    /// <summary>
    /// Minimum tutar
    /// </summary>
    [Range(0, 10000000)]
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// Maksimum tutar
    /// </summary>
    [Range(0, 10000000)]
    public decimal? MaxAmount { get; set; }

    /// <summary>
    /// Bolge filtreleri (virgul ile ayrilmis)
    /// </summary>
    [StringLength(500)]
    public string? Regions { get; set; }

    /// <summary>
    /// Arama terimi
    /// </summary>
    [StringLength(200)]
    public string? SearchTerm { get; set; }
}
