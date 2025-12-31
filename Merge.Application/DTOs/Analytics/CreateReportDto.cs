using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Analytics;

public class CreateReportDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Rapor adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    public Dictionary<string, object>? Filters { get; set; }
    
    [StringLength(50)]
    public string Format { get; set; } = "JSON";
}
