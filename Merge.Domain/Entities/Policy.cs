using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Entities;

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
    public ICollection<PolicyAcceptance> Acceptances { get; private set; } = new List<PolicyAcceptance>();

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

        if (content.Length < 10)
        {
            throw new DomainException("Policy içeriği en az 10 karakter olmalıdır.");
        }

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
        Title = newTitle;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyUpdatedEvent yayınla
        AddDomainEvent(new PolicyUpdatedEvent(Id, PolicyType, Version));
    }

    // ✅ BOLUM 1.1: Domain Method - Update content
    public void UpdateContent(string newContent)
    {
        Guard.AgainstNullOrEmpty(newContent, nameof(newContent));
        
        if (newContent.Length < 10)
        {
            throw new DomainException("Policy içeriği en az 10 karakter olmalıdır.");
        }

        Content = newContent;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyUpdatedEvent yayınla
        AddDomainEvent(new PolicyUpdatedEvent(Id, PolicyType, Version));
    }

    // ✅ BOLUM 1.1: Domain Method - Update version
    public void UpdateVersion(string newVersion)
    {
        Guard.AgainstNullOrEmpty(newVersion, nameof(newVersion));
        Version = newVersion;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyUpdatedEvent yayınla
        AddDomainEvent(new PolicyUpdatedEvent(Id, PolicyType, newVersion));
    }

    // ✅ BOLUM 1.1: Domain Method - Update change log
    public void UpdateChangeLog(string? newChangeLog)
    {
        ChangeLog = newChangeLog;
        UpdatedAt = DateTime.UtcNow;
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
    }

    // ✅ BOLUM 1.1: Domain Method - Activate
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyActivatedEvent yayınla
        AddDomainEvent(new PolicyActivatedEvent(Id, PolicyType, Version));
    }

    // ✅ BOLUM 1.1: Domain Method - Deactivate
    public void Deactivate()
    {
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
    }

    // ✅ BOLUM 1.1: Domain Method - Update created by user ID
    public void UpdateCreatedByUserId(Guid? createdByUserId)
    {
        CreatedByUserId = createdByUserId;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
        
        // ✅ BOLUM 1.5: Domain Events - PolicyDeletedEvent yayınla
        AddDomainEvent(new PolicyDeletedEvent(Id, PolicyType, Version));
    }
}
