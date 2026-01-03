using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Analytics;

/// <summary>
/// Create Report DTO - BOLUM 4.1: Validation Attributes (ZORUNLU)
/// </summary>
public class CreateReportDto : IValidatableObject
{
    [Required(ErrorMessage = "Rapor adı zorunludur")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Rapor adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Rapor tipi zorunludur")]
    [StringLength(100)]
    public string Type { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Başlangıç tarihi zorunludur")]
    public DateTime StartDate { get; set; }
    
    [Required(ErrorMessage = "Bitiş tarihi zorunludur")]
    public DateTime EndDate { get; set; }
    
    // ✅ BOLUM 4.3: Over-Posting Korumasi - Dictionary<string, object> YASAK
    // Typed DTO kullanılıyor
    public ReportFiltersDto? Filters { get; set; }
    
    [StringLength(50, ErrorMessage = "Format en fazla 50 karakter olabilir")]
    public string Format { get; set; } = "JSON";

    // ✅ BOLUM 4.1: Custom Validation - StartDate < EndDate
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate >= EndDate)
        {
            yield return new ValidationResult(
                "Başlangıç tarihi bitiş tarihinden önce olmalıdır",
                new[] { nameof(StartDate), nameof(EndDate) });
        }

        if (EndDate > DateTime.UtcNow)
        {
            yield return new ValidationResult(
                "Bitiş tarihi gelecekte olamaz",
                new[] { nameof(EndDate) });
        }

        // Type enum kontrolü
        if (!string.IsNullOrEmpty(Type) && !Enum.TryParse<Merge.Domain.Enums.ReportType>(Type, true, out _))
        {
            yield return new ValidationResult(
                "Geçersiz rapor tipi",
                new[] { nameof(Type) });
        }

        // Format enum kontrolü
        if (!string.IsNullOrEmpty(Format) && !Enum.TryParse<Merge.Domain.Enums.ReportFormat>(Format, true, out _))
        {
            yield return new ValidationResult(
                "Geçersiz format",
                new[] { nameof(Format) });
        }
    }
}
