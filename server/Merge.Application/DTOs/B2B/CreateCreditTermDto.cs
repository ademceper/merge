using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.B2B;

public class CreateCreditTermDto
{
    [Required]
    public Guid OrganizationId { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "İsim en az 2, en fazla 100 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Ödeme günü en az 1 olmalıdır.")]
    public int PaymentDays { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "Kredi limiti 0 veya daha büyük olmalıdır.")]
    public decimal? CreditLimit { get; set; }
    
    [StringLength(1000)]
    public string? Terms { get; set; }
}
