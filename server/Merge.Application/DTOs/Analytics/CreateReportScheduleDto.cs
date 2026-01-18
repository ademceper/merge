using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Analytics;

namespace Merge.Application.DTOs.Analytics;


public record CreateReportScheduleDto(
    [Required(ErrorMessage = "Zamanlama adı zorunludur")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Zamanlama adı en az 2, en fazla 200 karakter olmalıdır.")]
    string Name,
    
    [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
    string Description,
    
    [Required(ErrorMessage = "Rapor tipi zorunludur")]
    [StringLength(100)]
    string Type,
    
    [Required(ErrorMessage = "Sıklık zorunludur")]
    [StringLength(50)]
    string Frequency,
    
    [Range(1, 7, ErrorMessage = "Haftanın günü 1 (Pazartesi) ile 7 (Pazar) arasında olmalıdır.")]
    int DayOfWeek,
    
    [Range(1, 31, ErrorMessage = "Ayın günü 1 ile 31 arasında olmalıdır.")]
    int DayOfMonth,
    
    TimeSpan TimeOfDay,
    
    // Typed DTO kullanılıyor
    ReportFiltersDto? Filters,
    
    [StringLength(50, ErrorMessage = "Format en fazla 50 karakter olabilir")]
    string Format,
    
    [StringLength(1000, ErrorMessage = "E-posta alıcıları en fazla 1000 karakter olabilir")]
    [EmailAddress(ErrorMessage = "Geçerli e-posta adresleri giriniz (virgülle ayrılmış)")]
    string EmailRecipients
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Type enum kontrolü
        if (!string.IsNullOrEmpty(Type) && !Enum.TryParse<Merge.Domain.Enums.ReportType>(Type, true, out _))
        {
            yield return new ValidationResult(
                "Geçersiz rapor tipi",
                new[] { nameof(Type) });
        }

        // Frequency enum kontrolü
        if (!string.IsNullOrEmpty(Frequency))
        {
            if (!Enum.TryParse<Merge.Domain.Enums.ReportFrequency>(Frequency, true, out var frequency))
            {
                yield return new ValidationResult(
                    "Geçersiz sıklık",
                    new[] { nameof(Frequency) });
            }
            else
            {
                // Frequency'ye göre DayOfWeek/DayOfMonth kontrolü
                if (frequency == Merge.Domain.Enums.ReportFrequency.Weekly && (DayOfWeek < 1 || DayOfWeek > 7))
                {
                    yield return new ValidationResult(
                        "Haftalık raporlar için haftanın günü 1-7 arasında olmalıdır",
                        new[] { nameof(DayOfWeek) });
                }

                if (frequency == Merge.Domain.Enums.ReportFrequency.Monthly && (DayOfMonth < 1 || DayOfMonth > 31))
                {
                    yield return new ValidationResult(
                        "Aylık raporlar için ayın günü 1-31 arasında olmalıdır",
                        new[] { nameof(DayOfMonth) });
                }
            }
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
