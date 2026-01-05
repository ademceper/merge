using Merge.Domain.Common;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Entities;

// ✅ BOLUM 11.0: Rich Domain Model - Address entity (Anemik modelden Rich Domain Model'e çevrildi)
public class Address : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty; // Ev, İş, vb.
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string AddressLine1 { get; private set; } = string.Empty;
    public string? AddressLine2 { get; private set; }
    public string City { get; private set; } = string.Empty;
    public string District { get; private set; } = string.Empty;
    public string PostalCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = "Türkiye";
    public bool IsDefault { get; private set; } = false;
    
    // Navigation properties
    public User User { get; private set; } = null!;
    public ICollection<Order> Orders { get; private set; } = new List<Order>();

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Address() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Address Create(
        Guid userId,
        string title,
        string firstName,
        string lastName,
        string phoneNumber,
        string addressLine1,
        string city,
        string district,
        string postalCode,
        string country = "Türkiye",
        string? addressLine2 = null,
        bool isDefault = false)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstNullOrEmpty(addressLine1, nameof(addressLine1));
        Guard.AgainstNullOrEmpty(city, nameof(city));
        Guard.AgainstNullOrEmpty(firstName, nameof(firstName));
        Guard.AgainstNullOrEmpty(lastName, nameof(lastName));

        return new Address
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            FirstName = firstName,
            LastName = lastName,
            PhoneNumber = phoneNumber,
            AddressLine1 = addressLine1,
            AddressLine2 = addressLine2,
            City = city,
            District = district,
            PostalCode = postalCode,
            Country = country,
            IsDefault = isDefault,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ✅ BOLUM 1.1: Domain Logic - Update address
    public void UpdateAddress(
        string title,
        string firstName,
        string lastName,
        string phoneNumber,
        string addressLine1,
        string city,
        string district,
        string postalCode,
        string? addressLine2 = null)
    {
        Guard.AgainstNullOrEmpty(addressLine1, nameof(addressLine1));
        Guard.AgainstNullOrEmpty(city, nameof(city));
        Guard.AgainstNullOrEmpty(firstName, nameof(firstName));
        Guard.AgainstNullOrEmpty(lastName, nameof(lastName));

        Title = title;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        AddressLine1 = addressLine1;
        AddressLine2 = addressLine2;
        City = city;
        District = district;
        PostalCode = postalCode;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Set as default
    public void SetAsDefault()
    {
        if (IsDefault) return;
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Remove default flag
    public void RemoveDefault()
    {
        if (!IsDefault) return;
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

