using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record AnalyticsDateRangeDto(
    [Required] DateTime StartDate,
    [Required] DateTime EndDate,
    [StringLength(50)] string? ComparisonPeriod = null // PreviousPeriod, PreviousYear, Custom
);
