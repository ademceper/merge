using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Catalog;
using System.ComponentModel.DataAnnotations;

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
    public Guid? UserId { get; private set; }
    public User? User { get; private set; }
    public Guid? OrderId { get; private set; }
    public Order? Order { get; private set; }
    public Guid? PaymentId { get; private set; }
    public Payment? Payment { get; private set; }
    
    public FraudAlertType AlertType { get; private set; }
    
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
    
    public FraudAlertStatus Status { get; private set; } = FraudAlertStatus.Pending;
    
    public string? Reason { get; private set; } // Why this alert was triggered
    public Guid? ReviewedByUserId { get; private set; }
    public User? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public string? ReviewNotes { get; private set; }
    public string? MatchedRules { get; private set; } // JSON array of matched rule IDs

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private FraudAlert() { }

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

        alert.AddDomainEvent(new FraudAlertCreatedEvent(alert.Id, userId, alertType, riskScore));

        return alert;
    }

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

        AddDomainEvent(new FraudAlertReviewedEvent(Id, reviewedByUserId, status));
    }

    public void UpdateRiskScore(int riskScore)
    {
        Guard.AgainstOutOfRange(riskScore, 0, 100, nameof(riskScore));
        _riskScore = riskScore;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FraudAlertUpdatedEvent(Id, UserId, AlertType, riskScore, Status));
    }

    public void UpdateReason(string? reason)
    {
        Reason = reason;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FraudAlertUpdatedEvent(Id, UserId, AlertType, _riskScore, Status));
    }

    public void MarkAsDeleted()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FraudAlertDeletedEvent(Id, UserId, AlertType));
    }
}

