using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Analytics;

/// <summary>
/// Report ayarlari icin typed DTO - Dictionary yerine guvenli
/// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
/// </summary>
public record ReportSettingsDto(
    bool IsActive = true,
    [Range(1, 365)] int DefaultDateRangeDays = 30,
    [StringLength(50)] string? ChartType = null,
    [StringLength(20)] string? GroupBy = null,
    [Range(10, 1000)] int PageSize = 50,
    bool CsvExportEnabled = true,
    bool ExcelExportEnabled = true,
    bool PdfExportEnabled = false,
    bool EmailDeliveryEnabled = false,
    [StringLength(1000)] string? EmailRecipients = null,
    bool ScheduledEnabled = false,
    [StringLength(20)] string? ScheduleType = null,
    TimeSpan? ScheduleTime = null,
    [Range(0, 6)] int? ScheduleDayOfWeek = null,
    [Range(1, 31)] int? ScheduleDayOfMonth = null
);

/// <summary>
/// Report filtreleri icin typed DTO - Dictionary yerine guvenli
/// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
/// </summary>
public record ReportFiltersSettingsDto(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    [StringLength(1000)] string? CategoryIds = null,
    [StringLength(1000)] string? ProductIds = null,
    [StringLength(1000)] string? SellerIds = null,
    [StringLength(500)] string? Statuses = null,
    [Range(0, 10000000)] decimal? MinAmount = null,
    [Range(0, 10000000)] decimal? MaxAmount = null,
    [StringLength(500)] string? Regions = null,
    [StringLength(200)] string? SearchTerm = null
);
