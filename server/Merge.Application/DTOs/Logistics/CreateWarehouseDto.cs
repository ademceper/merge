using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.DTOs.Logistics;

public record CreateWarehouseDto(
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Depo adı en az 2, en fazla 200 karakter olmalıdır.")]
    string Name,
    
    [Required]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Kod en az 2, en fazla 50 karakter olmalıdır.")]
    string Code,
    
    [Required]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Adres en az 5, en fazla 500 karakter olmalıdır.")]
    string Address,
    
    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Şehir en az 2, en fazla 100 karakter olmalıdır.")]
    string City,
    
    [Required]
    [StringLength(100)]
    string Country,
    
    [StringLength(20)]
    string PostalCode,
    
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "İletişim kişisi adı en az 2, en fazla 200 karakter olmalıdır.")]
    string ContactPerson,
    
    [Required]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20)]
    string ContactPhone,
    
    [Required]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    string ContactEmail,
    
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Kapasite en az 1 olmalıdır.")]
    int Capacity,
    
    [StringLength(2000)]
    string? Description = null
);
