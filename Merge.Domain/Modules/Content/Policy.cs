using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Modules.Identity;

namespace Merge.Domain.Modules.Content;

/// <summary>
/// Policy Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'leri olduğu için IAggregateRoot
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// </summary>
public class Policy : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string PolicyType { get; private set; } = string.Empty; // TermsAndConditions, PrivacyPolicy, RefundPolicy, ShippingPolicy, etc.
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty; // HTML or Markdown content
    public string Version { get; private set; } = "1.0"; // Semantic versioning: 1.0, 1.1, 2.0, etc.
    public bool IsActive { get; private set; } = true;
    public bool RequiresAcceptance { get; private set; } = true; // Users must accept this policy
    public DateTime? EffectiveDate { get; private set; } // When this version becomes effective
    public DateTime? ExpiryDate { get; private set; } // When this version expires (optional)
    public Guid? CreatedByUserId { get; private set; } // Admin who created/updated
    public string? ChangeLog { get; private set; } // What changed in this version
    public string Language { get; private set; } = "tr"; // Multi-language support
    
    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public User? CreatedBy { get; private set; }
    
    // ✅ BOLUM 1.1: Encapsulated collection - Read-only access
    private readonly List<PolicyAcceptance> _acceptances = new();
    public IReadOnlyCollection<PolicyAcceptance> Acceptances => _acceptances.AsReadOnly();

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private Policy() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static Policy Create(
        string policyType,
        string title,
        string content,
        string version,
        Guid? createdByUserId = null,
        bool isActive = true,
        bool requiresAcceptance = true,
        DateTime? effectiveDate = null,
        DateTime? expiryDate = null,
        string? changeLog = null,
        string language = "tr")
    {
        Guard.AgainstNullOrEmpty(policyType, nameof(policyType));
        Guard.AgainstNullOrEmpty(title, nameof(title));
        Guard.AgainstNullOrEmpty(content, nameof(content));
        Guard.AgainstNullOrEmpty(version, nameof(version));
        Guard.AgainstNullOrEmpty(language, nameof(language));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MinPolicyContentLength=10, MaxPolicyTitleLength=200, MaxPolicyContentLength=50000, MaxPolicyTypeLength=50, MaxPolicyVersionLength=20, MaxLanguageCodeLength=10
        Guard.AgainstOutOfRange(content.Length, 10, int.MaxValue, nameof(content));
        Guard.AgainstLength(title, 200, nameof(title));
        Guard.AgainstLength(content, 50000, nameof(content));
        Guard.AgainstLength(policyType, 50, nameof(policyType));
        Guard.AgainstLength(version, 20, nameof(version));
        Guard.AgainstLength(language, 10, nameof(language));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxChangeLogLength=2000
        if (changeLog != null)
            Guard.AgainstLength(changeLog, 2000, nameof(changeLog));

        if (effectiveDate.HasValue && expiryDate.HasValue && effectiveDate.Value >= expiryDate.Value)
        {
            throw new DomainException("Effective date, expiry date'den önce olmalıdır.");
        }

        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            PolicyType = policyType,
            Title = title,
            Content = content,
            Version = version,
            IsActive = isActive,
            RequiresAcceptance = requiresAcceptance,
            EffectiveDate = effectiveDate ?? DateTime.UtcNow,
            ExpiryDate = expiryDate,
            CreatedByUserId = createdByUserId,
            ChangeLog = changeLog,
            Language = language,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - PolicyCreatedEvent yayınla
        policy.AddDomainEvent(new PolicyCreatedEvent(policy.Id, policyType, version, createdByUserId));

        return policy;
    }

    // ✅ BOLUM 1.1: Domain Method - Update title
    public void UpdateTitle(string newTitle)
    {
        Guard.AgainstNullOrEmpty(newTitle, nameof(newTitle));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxPolicyTitleLength=200
        Guard.AgainstLength(newTitle, 200, nameof(newTitle));
        Title = newTitle;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyUpdatedEvent yayınla
        AddDomainEvent(new PolicyUpdatedEvent(Id, PolicyType, Version));
    }

    // ✅ BOLUM 1.1: Domain Method - Update content
    public void UpdateContent(string newContent)
    {
        Guard.AgainstNullOrEmpty(newContent, nameof(newContent));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değerleri: MinPolicyContentLength=10, MaxPolicyContentLength=50000
        Guard.AgainstOutOfRange(newContent.Length, 10, int.MaxValue, nameof(newContent));
        Guard.AgainstLength(newContent, 50000, nameof(newContent));

        Content = newContent;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyUpdatedEvent yayınla
        AddDomainEvent(new PolicyUpdatedEvent(Id, PolicyType, Version));
    }

    // ✅ BOLUM 1.1: Domain Method - Update version
    public void UpdateVersion(string newVersion)
    {
        Guard.AgainstNullOrEmpty(newVersion, nameof(newVersion));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxPolicyVersionLength=20
        Guard.AgainstLength(newVersion, 20, nameof(newVersion));
        Version = newVersion;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyUpdatedEvent yayınla
        AddDomainEvent(new PolicyUpdatedEvent(Id, PolicyType, newVersion));
    }

    // ✅ BOLUM 1.1: Domain Method - Update change log
    public void UpdateChangeLog(string? newChangeLog)
    {
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxChangeLogLength=2000
        if (newChangeLog != null)
            Guard.AgainstLength(newChangeLog, 2000, nameof(newChangeLog));
        ChangeLog = newChangeLog;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyUpdatedEvent yayınla
        AddDomainEvent(new PolicyUpdatedEvent(Id, PolicyType, Version));
    }

    // ✅ BOLUM 1.1: Domain Method - Update effective date
    public void UpdateEffectiveDate(DateTime? newEffectiveDate)
    {
        if (newEffectiveDate.HasValue && ExpiryDate.HasValue && newEffectiveDate.Value >= ExpiryDate.Value)
        {
            throw new DomainException("Effective date, expiry date'den önce olmalıdır.");
        }

        EffectiveDate = newEffectiveDate;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyUpdatedEvent yayınla
        AddDomainEvent(new PolicyUpdatedEvent(Id, PolicyType, Version));
    }

    // ✅ BOLUM 1.1: Domain Method - Update expiry date
    public void UpdateExpiryDate(DateTime? newExpiryDate)
    {
        if (newExpiryDate.HasValue && EffectiveDate.HasValue && EffectiveDate.Value >= newExpiryDate.Value)
        {
            throw new DomainException("Expiry date, effective date'den sonra olmalıdır.");
        }

        ExpiryDate = newExpiryDate;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyUpdatedEvent yayınla
        AddDomainEvent(new PolicyUpdatedEvent(Id, PolicyType, Version));
    }

    // ✅ BOLUM 1.1: Domain Method - Activate
    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyActivatedEvent yayınla
        AddDomainEvent(new PolicyActivatedEvent(Id, PolicyType, Version));
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate
    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyDeactivatedEvent yayınla
        AddDomainEvent(new PolicyDeactivatedEvent(Id, PolicyType, Version));
    }

    // ✅ BOLUM 1.1: Domain Method - Update requires acceptance
    public void UpdateRequiresAcceptance(bool requiresAcceptance)
    {
        RequiresAcceptance = requiresAcceptance;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyUpdatedEvent yayınla
        AddDomainEvent(new PolicyUpdatedEvent(Id, PolicyType, Version));
    }

    // ✅ BOLUM 1.1: Domain Method - Update policy type
    public void UpdatePolicyType(string newPolicyType)
    {
        Guard.AgainstNullOrEmpty(newPolicyType, nameof(newPolicyType));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxPolicyTypeLength=50
        Guard.AgainstLength(newPolicyType, 50, nameof(newPolicyType));
        PolicyType = newPolicyType;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyUpdatedEvent yayınla
        AddDomainEvent(new PolicyUpdatedEvent(Id, newPolicyType, Version));
    }

    // ✅ BOLUM 1.1: Domain Method - Update language
    public void UpdateLanguage(string newLanguage)
    {
        Guard.AgainstNullOrEmpty(newLanguage, nameof(newLanguage));
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma - Entity'lerde sabit değerler kullanılıyor (Clean Architecture)
        // Configuration değeri: MaxLanguageCodeLength=10
        Guard.AgainstLength(newLanguage, 10, nameof(newLanguage));
        Language = newLanguage;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyUpdatedEvent yayınla
        AddDomainEvent(new PolicyUpdatedEvent(Id, PolicyType, Version));
    }

    // ✅ BOLUM 1.1: Domain Method - Update created by user ID
    public void UpdateCreatedByUserId(Guid? createdByUserId)
    {
        CreatedByUserId = createdByUserId;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyUpdatedEvent yayınla
        AddDomainEvent(new PolicyUpdatedEvent(Id, PolicyType, Version));
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyDeletedEvent yayınla
        AddDomainEvent(new PolicyDeletedEvent(Id, PolicyType, Version));
    }

    // ✅ BOLUM 1.1: Domain Method - Restore deleted policy
    public void Restore()
    {
        if (!IsDeleted)
            return;

        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyRestoredEvent yayınla (ÖNERİLİR)
        AddDomainEvent(new PolicyRestoredEvent(Id, PolicyType, Version));
    }
}
