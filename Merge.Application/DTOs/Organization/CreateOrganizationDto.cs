using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Organization;

public class CreateOrganizationDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Organizasyon adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? LegalName { get; set; }
    
    [StringLength(50)]
    public string? TaxNumber { get; set; }
    
    [StringLength(50)]
    public string? RegistrationNumber { get; set; }
    
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string? Email { get; set; }
    
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [StringLength(500)]
    [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
    public string? Website { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(100)]
    public string? State { get; set; }
    
    [StringLength(20)]
    public string? PostalCode { get; set; }
    
    [StringLength(100)]
    public string? Country { get; set; }
    
    public Dictionary<string, object>? Settings { get; set; }
}
