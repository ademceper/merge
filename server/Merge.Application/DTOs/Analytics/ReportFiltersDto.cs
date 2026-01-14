using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Analytics;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.DTOs.Analytics;

/// <summary>
/// Report Filters DTO - BOLUM 4.3: Over-Posting Korumasi (ZORUNLU)
/// Dictionary<string, object> yerine typed DTO kullanılıyor
/// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
/// </summary>
public record ReportFiltersDto(
    [StringLength(100)] string? Category = null,
    [StringLength(100)] string? Status = null,
    Guid? ProductId = null,
    Guid? CategoryId = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    [Range(0, double.MaxValue)] decimal? MinAmount = null,
    [Range(0, double.MaxValue)] decimal? MaxAmount = null,
    List<Guid>? ProductIds = null,
    List<Guid>? CategoryIds = null
);

