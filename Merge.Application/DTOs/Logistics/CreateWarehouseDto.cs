using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

public class CreateWarehouseDto
{
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Depo adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Kod en az 2, en fazla 50 karakter olmalıdır.")]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Adres en az 5, en fazla 500 karakter olmalıdır.")]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Şehir en az 2, en fazla 100 karakter olmalıdır.")]
    public string City { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string PostalCode { get; set; } = string.Empty;
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "İletişim kişisi adı en az 2, en fazla 200 karakter olmalıdır.")]
    public string ContactPerson { get; set; } = string.Empty;
    
    [Required]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20)]
    public string ContactPhone { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    public string ContactEmail { get; set; } = string.Empty;
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Kapasite en az 1 olmalıdır.")]
    public int Capacity { get; set; }
    
    [StringLength(2000)]
    public string? Description { get; set; }
}
