using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using Merge.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Ordering;

/// <summary>
/// OrderVerification Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.2: Enum kullanımı (ZORUNLU - String Status YASAK)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.6: Invariant Validation (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class OrderVerification : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid OrderId { get; private set; }
    
    // ✅ BOLUM 1.2: Enum kullanımı (string VerificationType YASAK)
    public VerificationType VerificationType { get; private set; }
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public VerificationStatus Status { get; private set; } = VerificationStatus.Pending;
    
    public Guid? VerifiedByUserId { get; private set; } // Admin/Staff who verified
    public DateTime? VerifiedAt { get; private set; }
    public string? VerificationNotes { get; private set; }
    public string? VerificationMethod { get; private set; } // Phone call, Email confirmation, ID check, etc.
    public bool RequiresManualReview { get; private set; } = false;
    
    // ✅ BOLUM 1.6: Invariant validation - RiskScore 0-100 arası
    private int _riskScore = 0;
    public int RiskScore 
    { 
        get => _riskScore; 
        private set 
        {
            Guard.AgainstOutOfRange(value, 0, 100, nameof(RiskScore));
            _riskScore = value;
        }
    }
    
    public string? RejectionReason { get; private set; }
    
    // ✅ BOLUM 1.7: Concurrency Control (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
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
    
    // Navigation properties
    public Order Order { get; private set; } = null!;
    public User? VerifiedBy { get; private set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private OrderVerification() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static OrderVerification Create(
        Guid orderId,
        VerificationType verificationType,
        int riskScore,
        string? verificationMethod = null,
        string? verificationNotes = null,
        bool requiresManualReview = false)
    {
        Guard.AgainstDefault(orderId, nameof(orderId));
        Guard.AgainstOutOfRange(riskScore, 0, 100, nameof(riskScore));

        var verification = new OrderVerification
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            VerificationType = verificationType,
            Status = VerificationStatus.Pending,
            VerificationMethod = verificationMethod,
            VerificationNotes = verificationNotes,
            RequiresManualReview = requiresManualReview,
            _riskScore = riskScore,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events
        verification.AddDomainEvent(new OrderVerificationCreatedEvent(
            verification.Id,
            orderId,
            verificationType,
            riskScore,
            verification.RequiresManualReview));

        return verification;
    }

    // ✅ BOLUM 1.1: Domain Method - Verify order
    public void Verify(Guid verifiedByUserId, string? notes = null)
    {
        Guard.AgainstDefault(verifiedByUserId, nameof(verifiedByUserId));

        if (Status != VerificationStatus.Pending)
            throw new DomainException("Only pending verifications can be verified");

        VerifiedByUserId = verifiedByUserId;
        VerifiedAt = DateTime.UtcNow;
        VerificationNotes = notes;
        Status = VerificationStatus.Verified;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new OrderVerificationVerifiedEvent(Id, OrderId, verifiedByUserId));
    }

    // ✅ BOLUM 1.1: Domain Method - Reject order
    public void Reject(Guid verifiedByUserId, string reason)
    {
        Guard.AgainstDefault(verifiedByUserId, nameof(verifiedByUserId));
        Guard.AgainstNullOrEmpty(reason, nameof(reason));

        if (Status != VerificationStatus.Pending)
            throw new DomainException("Only pending verifications can be rejected");

        VerifiedByUserId = verifiedByUserId;
        VerifiedAt = DateTime.UtcNow;
        RejectionReason = reason;
        Status = VerificationStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new OrderVerificationRejectedEvent(Id, OrderId, verifiedByUserId, reason));
    }

    // ✅ BOLUM 1.1: Domain Method - Update risk score
    // ✅ BOLUM 12.0: Magic number'lar handler'da belirlenir, Domain entity configuration'a bağımlı değil
    // RequiresManualReview handler'da belirlenmeli, bu method sadece risk score'u günceller
    public void UpdateRiskScore(int riskScore)
    {
        Guard.AgainstOutOfRange(riskScore, 0, 100, nameof(riskScore));
        _riskScore = riskScore;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update requires manual review
    public void SetRequiresManualReview(bool requiresManualReview)
    {
        RequiresManualReview = requiresManualReview;
        UpdatedAt = DateTime.UtcNow;
    }
}

