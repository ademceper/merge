using Merge.Domain.SharedKernel;
using System.ComponentModel.DataAnnotations;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Catalog;

namespace Merge.Domain.Modules.Payment;

/// <summary>
/// FraudAlert Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class FraudAlert : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid? UserId { get; private set; }
    public User? User { get; private set; }
    public Guid? OrderId { get; private set; }
    public Order? Order { get; private set; }
    public Guid? PaymentId { get; private set; }
    public Payment? Payment { get; private set; }
    
    // ✅ BOLUM 1.2: Enum kullanımı (string AlertType YASAK)
    public FraudAlertType AlertType { get; private set; }
    
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
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    public FraudAlertStatus Status { get; private set; } = FraudAlertStatus.Pending;
    
    public string? Reason { get; private set; } // Why this alert was triggered
    public Guid? ReviewedByUserId { get; private set; }
    public User? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? ReviewNotes { get; private set; }
    public string? MatchedRules { get; private set; } // JSON array of matched rule IDs

    // ✅ BOLUM 1.7: Concurrency Control - RowVersion (ZORUNLU)
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private FraudAlert() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static FraudAlert Create(
        Guid? userId,
        FraudAlertType alertType,
        int riskScore,
        string? reason = null,
        Guid? orderId = null,
        Guid? paymentId = null,
        string? matchedRules = null)
    {
        Guard.AgainstOutOfRange(riskScore, 0, 100, nameof(riskScore));

        var alert = new FraudAlert
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrderId = orderId,
            PaymentId = paymentId,
            AlertType = alertType,
            _riskScore = riskScore,
            Status = FraudAlertStatus.Pending,
            Reason = reason,
            MatchedRules = matchedRules,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - FraudAlertCreatedEvent
        alert.AddDomainEvent(new FraudAlertCreatedEvent(alert.Id, userId, alertType, riskScore));

        return alert;
    }

    // ✅ BOLUM 1.1: Domain Method - Review alert
    public void Review(Guid reviewedByUserId, FraudAlertStatus status, string? reviewNotes = null)
    {
        Guard.AgainstDefault(reviewedByUserId, nameof(reviewedByUserId));

        if (Status != FraudAlertStatus.Pending)
            throw new DomainException("Sadece bekleyen alert'ler gözden geçirilebilir");

        ReviewedByUserId = reviewedByUserId;
        ReviewedAt = DateTime.UtcNow;
        ReviewNotes = reviewNotes;
        Status = status;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - FraudAlertReviewedEvent
        AddDomainEvent(new FraudAlertReviewedEvent(Id, reviewedByUserId, status));
    }

    // ✅ BOLUM 1.1: Domain Method - Update risk score
    public void UpdateRiskScore(int riskScore)
    {
        Guard.AgainstOutOfRange(riskScore, 0, 100, nameof(riskScore));
        _riskScore = riskScore;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update reason
    public void UpdateReason(string? reason)
    {
        Reason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Mark as deleted (soft delete)
    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}

