using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Analytics;

public record AnalyticsDateRangeDto(
    [Required] DateTime StartDate,
    [Required] DateTime EndDate,
    [StringLength(50)] string? ComparisonPeriod = null // PreviousPeriod, PreviousYear, Custom
);
