using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Analytics;

public class CreateFinancialReportDto
{
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    public bool IncludeBreakdowns { get; set; } = true;
    
    public bool IncludeTrends { get; set; } = true;
}
