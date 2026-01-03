using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Analytics;

/// <summary>
/// Report Filters DTO - BOLUM 4.3: Over-Posting Korumasi (ZORUNLU)
/// Dictionary<string, object> yerine typed DTO kullanılıyor
/// </summary>
public class ReportFiltersDto
{
    [StringLength(100)]
    public string? Category { get; set; }

    [StringLength(100)]
    public string? Status { get; set; }

    public Guid? ProductId { get; set; }

    public Guid? CategoryId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MinAmount { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? MaxAmount { get; set; }

    public List<Guid>? ProductIds { get; set; }

    public List<Guid>? CategoryIds { get; set; }
}

