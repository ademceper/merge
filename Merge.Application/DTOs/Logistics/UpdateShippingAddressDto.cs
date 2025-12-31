using System.ComponentModel.DataAnnotations;

namespace Merge.Application.DTOs.Logistics;

public class UpdateShippingAddressDto
{
    [StringLength(50)]
    public string? Label { get; set; }
    
    [StringLength(100, MinimumLength = 2)]
    public string? FirstName { get; set; }
    
    [StringLength(100, MinimumLength = 2)]
    public string? LastName { get; set; }
    
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [StringLength(20)]
    public string? Phone { get; set; }
    
    [StringLength(200, MinimumLength = 5)]
    public string? AddressLine1 { get; set; }
    
    [StringLength(200)]
    public string? AddressLine2 { get; set; }
    
    [StringLength(100, MinimumLength = 2)]
    public string? City { get; set; }
    
    [StringLength(100)]
    public string? State { get; set; }
    
    [StringLength(20)]
    public string? PostalCode { get; set; }
    
    [StringLength(100)]
    public string? Country { get; set; }
    
    public bool? IsDefault { get; set; }
    
    public bool? IsActive { get; set; }
    
    [StringLength(500)]
    public string? Instructions { get; set; }
}
