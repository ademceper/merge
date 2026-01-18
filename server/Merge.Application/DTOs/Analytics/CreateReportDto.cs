using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Analytics;

namespace Merge.Application.DTOs.Analytics;


public record CreateReportDto(
    [Required(ErrorMessage = "Rapor adı zorunludur")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Rapor adı en az 2, en fazla 200 karakter olmalıdır.")]
    string Name,
    
    [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    string Description,
    
    [Required(ErrorMessage = "Rapor tipi zorunludur")]
    [StringLength(100)]
    string Type,
    
    [Required(ErrorMessage = "Başlangıç tarihi zorunludur")]
    DateTime StartDate,
    
    [Required(ErrorMessage = "Bitiş tarihi zorunludur")]
    DateTime EndDate,
    
    // Typed DTO kullanılıyor
    ReportFiltersDto? Filters,
    
    [StringLength(50, ErrorMessage = "Format en fazla 50 karakter olabilir")]
    string Format = "JSON"
) : IValidatableObject
{
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
