using Merge.Domain.Common;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;

namespace Merge.Domain.Entities;

/// <summary>
/// CreditTerm Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class CreditTerm : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty; // e.g., "Net 30", "Net 60"
    public int PaymentDays { get; private set; } // Number of days to pay
    
    // ✅ BOLUM 1.3: Value Objects kullanımı - EF Core compatibility için decimal backing fields
    private decimal? _creditLimit;
    private decimal? _usedCredit;
    
    // Database columns (EF Core mapping)
    public decimal? CreditLimit 
    { 
        get => _creditLimit; 
        private set 
        {
            if (value.HasValue)
            {
                Guard.AgainstNegative(value.Value, nameof(CreditLimit));
            }
            _creditLimit = value;
        }
    }
    
    public decimal? UsedCredit 
    { 
        get => _usedCredit; 
        private set 
        {
            if (value.HasValue)
            {
                Guard.AgainstNegative(value.Value, nameof(UsedCredit));
            }
            _usedCredit = value;
        }
    }
    
    // ✅ BOLUM 1.3: Value Object properties (computed from decimal)
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money? CreditLimitMoney => _creditLimit.HasValue ? new Money(_creditLimit.Value) : null;
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money? UsedCreditMoney => _usedCredit.HasValue ? new Money(_usedCredit.Value) : null;
    
    public bool IsActive { get; private set; } = true;
    public string? Terms { get; private set; } // Additional terms description
    
    // ✅ BOLUM 1.5: Concurrency Control
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Organization Organization { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private CreditTerm() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static CreditTerm Create(
        Guid organizationId,
        Organization organization,
        string name,
        int paymentDays,
        decimal? creditLimit = null,
        string? terms = null)
    {
        Guard.AgainstDefault(organizationId, nameof(organizationId));
        Guard.AgainstNull(organization, nameof(organization));
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNegativeOrZero(paymentDays, nameof(paymentDays));

        var creditTerm = new CreditTerm
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Organization = organization,
            Name = name,
            PaymentDays = paymentDays,
            CreditLimit = creditLimit,
            UsedCredit = 0,
            IsActive = true,
            Terms = terms,
            CreatedAt = DateTime.UtcNow
        };

        return creditTerm;
    }

    // ✅ BOLUM 1.1: Domain Logic - Use credit
    public void UseCredit(decimal amount)
    {
        Guard.AgainstNegativeOrZero(amount, nameof(amount));

        if (!CreditLimit.HasValue)
            throw new DomainException("Kredi limiti tanımlı değil");

        var newUsedCredit = (UsedCredit ?? 0) + amount;

        // ✅ BOLUM 1.6: Invariant validation
        if (newUsedCredit > CreditLimit.Value)
            throw new DomainException("Kredi limiti aşıldı");

        UsedCredit = newUsedCredit;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Release credit
    public void ReleaseCredit(decimal amount)
    {
        Guard.AgainstNegativeOrZero(amount, nameof(amount));

        var newUsedCredit = (UsedCredit ?? 0) - amount;

        // ✅ BOLUM 1.6: Invariant validation
        if (newUsedCredit < 0)
            throw new DomainException("Kullanılan kredi negatif olamaz");

        UsedCredit = newUsedCredit;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update credit limit
    public void UpdateCreditLimit(decimal? creditLimit)
    {
        if (creditLimit.HasValue)
        {
            Guard.AgainstNegative(creditLimit.Value, nameof(creditLimit));
            
            // ✅ BOLUM 1.6: Invariant validation
            if (UsedCredit.HasValue && UsedCredit.Value > creditLimit.Value)
                throw new DomainException("Kullanılan kredi, kredi limitinden büyük olamaz");
        }

        CreditLimit = creditLimit;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Activate/Deactivate
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update details
    public void UpdateDetails(string name, int paymentDays, string? terms = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNegativeOrZero(paymentDays, nameof(paymentDays));

        Name = name;
        PaymentDays = paymentDays;
        Terms = terms;
        UpdatedAt = DateTime.UtcNow;
    }
}

