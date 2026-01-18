using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.DTOs.Logistics;

public record UpdateWarehouseDto(
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Depo adı en az 2, en fazla 200 karakter olmalıdır.")]
    string? Name = null,
    
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Adres en az 5, en fazla 500 karakter olmalıdır.")]
    string? Address = null,
    
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Şehir en az 2, en fazla 100 karakter olmalıdır.")]
    string? City = null,
    
    [StringLength(100)]
    string? Country = null,
    
    [StringLength(20)]
    string? PostalCode = null,
    
    [StringLength(200, MinimumLength = 2, ErrorMessage = "İletişim kişisi adı en az 2, en fazla 200 karakter olmalıdır.")]
    string? ContactPerson = null,
    
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20)]
    string? ContactPhone = null,
    
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [StringLength(200)]
    string? ContactEmail = null,
    
    [Range(1, int.MaxValue, ErrorMessage = "Kapasite en az 1 olmalıdır.")]
    int? Capacity = null,
    
    bool? IsActive = null,
    
    [StringLength(2000)]
    string? Description = null
);
