using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Payment;

/// <summary>
/// CreditTerm Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class CreditTerm : BaseEntity, IAggregateRoot
{
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = string.Empty; // e.g., "Net 30", "Net 60"
    public int PaymentDays { get; private set; } // Number of days to pay
    
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
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money? CreditLimitMoney => _creditLimit.HasValue ? new Money(_creditLimit.Value) : null;
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Money? UsedCreditMoney => _usedCredit.HasValue ? new Money(_usedCredit.Value) : null;
    
    public bool IsActive { get; private set; } = true;
    public string? Terms { get; private set; } // Additional terms description
    
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Organization Organization { get; private set; } = null!;

    private CreditTerm() { }

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

        creditTerm.AddDomainEvent(new CreditTermCreatedEvent(
            creditTerm.Id,
            organizationId,
            name,
            paymentDays,
            creditLimit));

        return creditTerm;
    }

    public void UseCredit(decimal amount)
    {
        Guard.AgainstNegativeOrZero(amount, nameof(amount));

        if (!CreditLimit.HasValue)
            throw new DomainException("Kredi limiti tanımlı değil");

        var newUsedCredit = (UsedCredit ?? 0) + amount;

        if (newUsedCredit > CreditLimit.Value)
            throw new DomainException("Kredi limiti aşıldı");

        UsedCredit = newUsedCredit;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CreditTermCreditUsedEvent(Id, OrganizationId, amount, newUsedCredit));
    }

    public void ReleaseCredit(decimal amount)
    {
        Guard.AgainstNegativeOrZero(amount, nameof(amount));

        var newUsedCredit = (UsedCredit ?? 0) - amount;

        if (newUsedCredit < 0)
            throw new DomainException("Kullanılan kredi negatif olamaz");

        UsedCredit = newUsedCredit;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CreditTermCreditReleasedEvent(Id, OrganizationId, amount, newUsedCredit));
    }

    public void UpdateCreditLimit(decimal? creditLimit)
    {
        if (creditLimit.HasValue)
        {
            Guard.AgainstNegative(creditLimit.Value, nameof(creditLimit));
            
            if (UsedCredit.HasValue && UsedCredit.Value > creditLimit.Value)
                throw new DomainException("Kullanılan kredi, kredi limitinden büyük olamaz");
        }

        CreditLimit = creditLimit;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CreditTermUpdatedEvent(Id, OrganizationId, Name, PaymentDays, creditLimit));
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CreditTermActivatedEvent(Id, OrganizationId));
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CreditTermDeactivatedEvent(Id, OrganizationId));
    }

    public void UpdateDetails(string name, int paymentDays, string? terms = null)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNegativeOrZero(paymentDays, nameof(paymentDays));

        Name = name;
        PaymentDays = paymentDays;
        Terms = terms;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CreditTermUpdatedEvent(Id, OrganizationId, name, paymentDays, CreditLimit));
    }

    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("Credit term zaten silinmiş");

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CreditTermDeletedEvent(Id, OrganizationId));
    }
}

