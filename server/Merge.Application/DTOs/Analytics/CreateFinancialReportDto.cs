using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Analytics;

public record CreateFinancialReportDto(
    [Required] DateTime StartDate,
    [Required] DateTime EndDate,
    bool IncludeBreakdowns = true,
    bool IncludeTrends = true
);
