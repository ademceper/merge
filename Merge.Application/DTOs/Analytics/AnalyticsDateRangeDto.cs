using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Analytics;

public class AnalyticsDateRangeDto
{
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    [StringLength(50)]
    public string? ComparisonPeriod { get; set; } // PreviousPeriod, PreviousYear, Custom
}
