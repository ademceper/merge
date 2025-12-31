namespace Merge.Domain.Entities;

public class ShippingAddress : BaseEntity
{
    public Guid UserId { get; set; }
    public string Label { get; set; } = string.Empty; // Home, Work, Other, etc.
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? Instructions { get; set; } // Delivery instructions
    
    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}

