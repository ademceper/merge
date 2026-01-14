using Merge.Domain.SharedKernel;
using Merge.Domain.Enums;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using Merge.Domain.ValueObjects;
using Merge.Domain.Modules.Ordering;

namespace Merge.Domain.Modules.Identity;

/// <summary>
/// B2BUser Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// </summary>
public class B2BUser : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid UserId { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string? EmployeeId { get; private set; } // Company employee ID
    public string? Department { get; private set; }
    public string? JobTitle { get; private set; }
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    public EntityStatus Status { get; private set; } = EntityStatus.Active;
    
    public bool IsApproved { get; private set; } = false;
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    
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
    
    public string? Settings { get; private set; } // JSON for B2B-specific settings
    
    // ✅ BOLUM 1.7: Concurrency Control - [Timestamp] RowVersion (ZORUNLU)
    // Kredi limiti kullanımı için concurrency control gerekli
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public User User { get; private set; } = null!;
    public Organization Organization { get; private set; } = null!;
    public User? ApprovedBy { get; private set; }
    public ICollection<PurchaseOrder> PurchaseOrders { get; private set; } = new List<PurchaseOrder>();

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.AddDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.4: IAggregateRoot interface implementation - Remove domain event
    public new void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        if (domainEvent == null)
            throw new ArgumentNullException(nameof(domainEvent));
        
        base.RemoveDomainEvent(domainEvent);
    }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private B2BUser() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static B2BUser Create(
        Guid userId,
        Guid organizationId,
        Organization organization,
        string? employeeId = null,
        string? department = null,
        string? jobTitle = null,
        decimal? creditLimit = null)
    {
        Guard.AgainstDefault(userId, nameof(userId));
        Guard.AgainstDefault(organizationId, nameof(organizationId));
        Guard.AgainstNull(organization, nameof(organization));
        
        if (!string.IsNullOrEmpty(employeeId))
        {
            Guard.AgainstLength(employeeId, 100, nameof(employeeId));
        }
        
        if (!string.IsNullOrEmpty(department))
        {
            Guard.AgainstLength(department, 200, nameof(department));
        }
        
        if (!string.IsNullOrEmpty(jobTitle))
        {
            Guard.AgainstLength(jobTitle, 200, nameof(jobTitle));
        }

        var b2bUser = new B2BUser
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrganizationId = organizationId,
            Organization = organization,
            EmployeeId = employeeId,
            Department = department,
            JobTitle = jobTitle,
            Status = EntityStatus.Active,
            IsApproved = false,
            CreditLimit = creditLimit,
            UsedCredit = 0,
            CreatedAt = DateTime.UtcNow
        };
        
        // Settings validation (eğer varsa)
        // Settings JSON olarak saklanıyor, bu yüzden length validation eklenebilir
        // Ancak Create metodunda Settings parametresi yok, bu yüzden şimdilik atlıyoruz

        // ✅ BOLUM 1.5: Domain Event - B2B User Created
        b2bUser.AddDomainEvent(new B2BUserCreatedEvent(
            b2bUser.Id,
            userId,
            organizationId,
            employeeId,
            department,
            jobTitle,
            creditLimit));

        return b2bUser;
    }

    // ✅ BOLUM 1.1: Domain Logic - Approve B2B user
    public void Approve(Guid approvedByUserId)
    {
        Guard.AgainstDefault(approvedByUserId, nameof(approvedByUserId));

        if (IsApproved)
            throw new DomainException("B2B kullanıcı zaten onaylanmış");

        IsApproved = true;
        ApprovedAt = DateTime.UtcNow;
        ApprovedByUserId = approvedByUserId;
        Status = EntityStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - B2B User Approved
        AddDomainEvent(new B2BUserApprovedEvent(Id, UserId, OrganizationId, approvedByUserId));
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

        // ✅ BOLUM 1.5: Domain Event - B2B User Updated
        AddDomainEvent(new B2BUserUpdatedEvent(Id, UserId, OrganizationId));
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

        // ✅ BOLUM 1.5: Domain Event - B2B User Credit Used
        AddDomainEvent(new B2BUserCreditUsedEvent(Id, UserId, OrganizationId, amount, newUsedCredit));
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

        // ✅ BOLUM 1.5: Domain Event - B2B User Credit Released
        AddDomainEvent(new B2BUserCreditReleasedEvent(Id, UserId, OrganizationId, amount, newUsedCredit));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update profile
    public void UpdateProfile(string? employeeId, string? department, string? jobTitle)
    {
        if (!string.IsNullOrEmpty(employeeId))
        {
            Guard.AgainstLength(employeeId, 100, nameof(employeeId));
        }
        
        if (!string.IsNullOrEmpty(department))
        {
            Guard.AgainstLength(department, 200, nameof(department));
        }
        
        if (!string.IsNullOrEmpty(jobTitle))
        {
            Guard.AgainstLength(jobTitle, 200, nameof(jobTitle));
        }
        
        EmployeeId = employeeId;
        Department = department;
        JobTitle = jobTitle;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - B2B User Updated
        AddDomainEvent(new B2BUserUpdatedEvent(Id, UserId, OrganizationId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update status
    public void UpdateStatus(EntityStatus status)
    {
        if (Status == status)
            return;

        Status = status;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - B2B User Updated
        AddDomainEvent(new B2BUserUpdatedEvent(Id, UserId, OrganizationId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Update settings
    public void UpdateSettings(string? settingsJson)
    {
        if (!string.IsNullOrEmpty(settingsJson))
        {
            Guard.AgainstLength(settingsJson, 2000, nameof(settingsJson));
        }
        
        Settings = settingsJson;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - B2B User Updated
        AddDomainEvent(new B2BUserUpdatedEvent(Id, UserId, OrganizationId));
    }

    // ✅ BOLUM 1.1: Domain Logic - Delete (soft delete)
    public void Delete()
    {
        if (IsDeleted)
            throw new DomainException("B2B kullanıcı zaten silinmiş");

        IsDeleted = true;
        Status = EntityStatus.Deleted;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Event - B2B User Deleted
        AddDomainEvent(new B2BUserDeletedEvent(Id, UserId, OrganizationId));
    }
}
