using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.B2B;

public class CreateB2BUserDto
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid OrganizationId { get; set; }
    
    [StringLength(50)]
    public string? EmployeeId { get; set; }
    
    [StringLength(100)]
    public string? Department { get; set; }
    
    [StringLength(100)]
    public string? JobTitle { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Kredi limiti 0 veya daha büyük olmalıdır.")]
    public decimal? CreditLimit { get; set; }
    
    // Typed DTO kullanılıyor
    public B2BUserSettingsDto? Settings { get; set; }
}
