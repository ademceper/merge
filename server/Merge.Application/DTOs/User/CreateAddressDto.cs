using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;

namespace Merge.Application.DTOs.User;

public class CreateAddressDto
{
    [Required]
    public Guid UserId { get; set; }

    [StringLength(50)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Ad en az 2, en fazla 100 karakter olmalıdır.")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Soyad en az 2, en fazla 100 karakter olmalıdır.")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 5, ErrorMessage = "Adres satırı en az 5, en fazla 200 karakter olmalıdır.")]
    public string AddressLine1 { get; set; } = string.Empty;

    [StringLength(200)]
    public string? AddressLine2 { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Şehir en az 2, en fazla 100 karakter olmalıdır.")]
    public string City { get; set; } = string.Empty;

    [StringLength(100)]
    public string District { get; set; } = string.Empty;

    [StringLength(10)]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Country { get; set; } = "Türkiye";

    public bool IsDefault { get; set; } = false;
}

