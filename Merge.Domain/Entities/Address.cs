namespace Merge.Domain.Entities;

public class Address : BaseEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty; // Ev, İş, vb.
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "Türkiye";
    public bool IsDefault { get; set; } = false;
    
    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

