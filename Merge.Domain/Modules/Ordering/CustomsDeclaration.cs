using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.Modules.Payment;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// CustomsDeclaration Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.2: Enum kullanımı (ZORUNLU - String Status YASAK)
/// BOLUM 1.3: Value Objects (ZORUNLU) - Money Value Object kullanımı
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.6: Invariant Validation (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class CustomsDeclaration : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid OrderId { get; private set; }
    public string DeclarationNumber { get; private set; } = string.Empty;
    public string OriginCountry { get; private set; } = string.Empty;
    public string DestinationCountry { get; private set; } = string.Empty;
    
    // ✅ BOLUM 1.3: Value Objects kullanımı - EF Core compatibility için decimal backing fields
    private decimal _totalValue;
    private decimal? _customsDuty;
    private decimal? _importTax;
    private decimal _weight;
    
    // Database columns (EF Core mapping)
    public decimal TotalValue 
    { 
        get => _totalValue; 
        private set 
        {
            Guard.AgainstNegativeOrZero(value, nameof(TotalValue));
            _totalValue = value;
        }
    }
    
    public decimal? CustomsDuty 
    { 
        get => _customsDuty; 
        private set 
        {
            if (value.HasValue)
                Guard.AgainstNegative(value.Value, nameof(CustomsDuty));
            _customsDuty = value;
        }
    }
    
    public decimal? ImportTax 
    { 
        get => _importTax; 
        private set 
        {
            if (value.HasValue)
                Guard.AgainstNegative(value.Value, nameof(ImportTax));
            _importTax = value;
        }
    }
    
    public decimal Weight 
    { 
        get => _weight; 
        private set 
        {
            Guard.AgainstNegativeOrZero(value, nameof(Weight));
            _weight = value;
        }
    }
    
    // ✅ BOLUM 1.3: Value Object properties (computed from decimal)
    [NotMapped]
    public Money TotalValueMoney => new Money(_totalValue);
    
    [NotMapped]
    public Money? CustomsDutyMoney => _customsDuty.HasValue ? new Money(_customsDuty.Value) : null;
    
    [NotMapped]
    public Money? ImportTaxMoney => _importTax.HasValue ? new Money(_importTax.Value) : null;
    
    public string Currency { get; private set; } = "USD";
    public string? HsCode { get; private set; } // Harmonized System code
    public string? Description { get; private set; }
    
    private int _quantity;
    public int Quantity 
    { 
        get => _quantity; 
        private set 
        {
            Guard.AgainstNegativeOrZero(value, nameof(Quantity));
            _quantity = value;
        }
    }
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    public VerificationStatus Status { get; private set; } = VerificationStatus.Pending;
    
    public DateTime? SubmittedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? Documents { get; private set; } // JSON array of document URLs
    
    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    // BaseEntity'deki protected AddDomainEvent yerine public AddDomainEvent kullanılabilir
    // Service layer'dan event eklenebilmesi için public yapıldı
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected AddDomainEvent'i çağır
        base.AddDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    // BaseEntity'deki protected RemoveDomainEvent yerine public RemoveDomainEvent kullanılabilir
    // Service layer'dan event kaldırılabilmesi için public yapıldı
    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        // BaseEntity'deki protected RemoveDomainEvent'i çağır
        base.RemoveDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.7: Concurrency Control
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Navigation properties
    public Order Order { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private CustomsDeclaration() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static CustomsDeclaration Create(
        Guid orderId,
        string declarationNumber,
        string originCountry,
        string destinationCountry,
        Money totalValue,
        decimal weight,
        int quantity,
        string currency = "USD",
        string? hsCode = null,
        string? description = null)
    {
        Guard.AgainstDefault(orderId, nameof(orderId));
        Guard.AgainstNullOrEmpty(declarationNumber, nameof(declarationNumber));
        Guard.AgainstNullOrEmpty(originCountry, nameof(originCountry));
        Guard.AgainstNullOrEmpty(destinationCountry, nameof(destinationCountry));
        Guard.AgainstNull(totalValue, nameof(totalValue));
        Guard.AgainstNegativeOrZero(weight, nameof(weight));
        Guard.AgainstNegativeOrZero(quantity, nameof(quantity));
        Guard.AgainstNullOrEmpty(currency, nameof(currency));

        var declaration = new CustomsDeclaration
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            DeclarationNumber = declarationNumber,
            OriginCountry = originCountry,
            DestinationCountry = destinationCountry,
            _totalValue = totalValue.Amount,
            _weight = weight,
            _quantity = quantity, // EF Core compatibility - backing field
            Currency = currency,
            HsCode = hsCode,
            Description = description,
            Status = VerificationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - CustomsDeclarationCreatedEvent
        declaration.AddDomainEvent(new CustomsDeclarationCreatedEvent(
            declaration.Id,
            orderId,
            declarationNumber,
            totalValue.Amount,
            currency));

        return declaration;
    }

    // ✅ BOLUM 1.1: Domain Method - Submit declaration
    public void Submit()
    {
        if (Status != VerificationStatus.Pending)
            throw new DomainException("Sadece bekleyen gümrük beyannameleri gönderilebilir");

        // Status remains Pending until approved/rejected
        // SubmittedAt timestamp indicates submission
        SubmittedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CustomsDeclarationSubmittedEvent
        AddDomainEvent(new CustomsDeclarationSubmittedEvent(Id, OrderId, DeclarationNumber));
    }

    // ✅ BOLUM 1.1: Domain Method - Approve declaration
    public void Approve()
    {
        if (Status != VerificationStatus.Pending)
            throw new DomainException("Sadece bekleyen gümrük beyannameleri onaylanabilir");

        Status = VerificationStatus.Verified;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CustomsDeclarationApprovedEvent
        AddDomainEvent(new CustomsDeclarationApprovedEvent(Id, OrderId, DeclarationNumber));
    }

    // ✅ BOLUM 1.1: Domain Method - Reject declaration
    public void Reject(string reason)
    {
        Guard.AgainstNullOrEmpty(reason, nameof(reason));

        if (Status != VerificationStatus.Pending)
            throw new DomainException("Sadece bekleyen gümrük beyannameleri reddedilebilir");

        Status = VerificationStatus.Rejected;
        RejectionReason = reason;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CustomsDeclarationRejectedEvent
        AddDomainEvent(new CustomsDeclarationRejectedEvent(Id, OrderId, DeclarationNumber, reason));
    }

    // ✅ BOLUM 1.1: Domain Method - Update customs duty
    public void UpdateCustomsDuty(Money customsDuty)
    {
        Guard.AgainstNull(customsDuty, nameof(customsDuty));

        _customsDuty = customsDuty.Amount;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CustomsDeclarationUpdatedEvent
        AddDomainEvent(new CustomsDeclarationUpdatedEvent(Id, OrderId, "CustomsDuty"));
    }

    // ✅ BOLUM 1.1: Domain Method - Update import tax
    public void UpdateImportTax(Money importTax)
    {
        Guard.AgainstNull(importTax, nameof(importTax));

        _importTax = importTax.Amount;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CustomsDeclarationUpdatedEvent
        AddDomainEvent(new CustomsDeclarationUpdatedEvent(Id, OrderId, "ImportTax"));
    }

    // ✅ BOLUM 1.1: Domain Method - Update documents
    public void UpdateDocuments(string documents)
    {
        Guard.AgainstNullOrEmpty(documents, nameof(documents));

        Documents = documents;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - CustomsDeclarationUpdatedEvent
        AddDomainEvent(new CustomsDeclarationUpdatedEvent(Id, OrderId, "Documents"));
    }
}
