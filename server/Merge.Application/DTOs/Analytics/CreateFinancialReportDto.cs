using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record CreateFinancialReportDto(
    [Required] DateTime StartDate,
    [Required] DateTime EndDate,
    bool IncludeBreakdowns = true,
    bool IncludeTrends = true
);
