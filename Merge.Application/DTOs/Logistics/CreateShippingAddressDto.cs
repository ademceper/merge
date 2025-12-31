using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

public class CreateShippingAddressDto
{
    [StringLength(50)]
    public string Label { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad en az 2, en fazla 100 karakter olmalıdır.")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Soyad en az 2, en fazla 100 karakter olmalıdır.")]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20)]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Adres satırı en az 5, en fazla 200 karakter olmalıdır.")]
    public string AddressLine1 { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? AddressLine2 { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Şehir en az 2, en fazla 100 karakter olmalıdır.")]
    public string City { get; set; } = string.Empty;
    
    [StringLength(100)]
    public string State { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string PostalCode { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;
    
    public bool IsDefault { get; set; } = false;
    
    [StringLength(500)]
    public string? Instructions { get; set; }
}
