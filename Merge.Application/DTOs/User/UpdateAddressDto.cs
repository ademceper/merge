using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.User;

public class UpdateAddressDto
{
    [StringLength(50)]
    public string Title { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 2)]
    public string FirstName { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 2)]
    public string LastName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(200, MinimumLength = 5)]
    public string AddressLine1 { get; set; } = string.Empty;

    [StringLength(200)]
    public string? AddressLine2 { get; set; }

    [StringLength(100, MinimumLength = 2)]
    public string City { get; set; } = string.Empty;

    [StringLength(100)]
    public string District { get; set; } = string.Empty;

    [StringLength(10)]
    public string PostalCode { get; set; } = string.Empty;

    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    public bool IsDefault { get; set; } = false;
}

