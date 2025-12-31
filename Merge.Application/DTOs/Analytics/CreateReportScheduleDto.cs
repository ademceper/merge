using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Analytics;

public class CreateReportScheduleDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Zamanlama adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50)]
    public string Frequency { get; set; } = string.Empty;
    
    [Range(0, 6, ErrorMessage = "Haftanın günü 0 ile 6 arasında olmalıdır.")]
    public int DayOfWeek { get; set; } = 1;
    
    [Range(1, 31, ErrorMessage = "Ayın günü 1 ile 31 arasında olmalıdır.")]
    public int DayOfMonth { get; set; } = 1;
    
    public TimeSpan TimeOfDay { get; set; } = TimeSpan.Zero;
    
    public Dictionary<string, object>? Filters { get; set; }
    
    [StringLength(50)]
    public string Format { get; set; } = "PDF";
    
    [StringLength(1000)]
    public string EmailRecipients { get; set; } = string.Empty;
}
