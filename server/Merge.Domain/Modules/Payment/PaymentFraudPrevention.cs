using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Enums;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Payment;

/// <summary>
/// PaymentFraudPrevention Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class PaymentFraudPrevention : BaseEntity, IAggregateRoot
{
    public Guid PaymentId { get; private set; }
    
    public PaymentCheckType CheckType { get; private set; }
    
    public VerificationStatus Status { get; private set; } = VerificationStatus.Pending;
    
    public bool IsBlocked { get; private set; } = false;
    public string? BlockReason { get; private set; }
    
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
    
    public string? CheckResult { get; private set; } // JSON for detailed results
    public DateTime? CheckedAt { get; private set; }
    public string? DeviceFingerprint { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation properties
    public Payment Payment { get; private set; } = null!;

    private PaymentFraudPrevention() { }

    public static PaymentFraudPrevention Create(
        Guid paymentId,
        PaymentCheckType checkType,
        int riskScore,
        VerificationStatus status,
        bool isBlocked = false,
        string? blockReason = null,
        string? checkResult = null,
        string? deviceFingerprint = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        Guard.AgainstDefault(paymentId, nameof(paymentId));
        Guard.AgainstOutOfRange(riskScore, 0, 100, nameof(riskScore));

        var check = new PaymentFraudPrevention
        {
            Id = Guid.NewGuid(),
            PaymentId = paymentId,
            CheckType = checkType,
            Status = status,
            IsBlocked = isBlocked,
            BlockReason = blockReason,
            _riskScore = riskScore,
            CheckResult = checkResult,
            CheckedAt = DateTime.UtcNow,
            DeviceFingerprint = deviceFingerprint,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow
        };

        check.AddDomainEvent(new PaymentFraudPreventionCreatedEvent(
            check.Id,
            paymentId,
            checkType,
            riskScore,
            status));

        return check;
    }

    public void Block(string reason)
    {
        Guard.AgainstNullOrEmpty(reason, nameof(reason));

        if (IsBlocked)
            throw new DomainException("Payment is already blocked");

        IsBlocked = true;
        BlockReason = reason;
        Status = VerificationStatus.Failed;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentFraudPreventionBlockedEvent(Id, PaymentId, reason));
    }

    public void Unblock()
    {
        if (!IsBlocked)
            throw new DomainException("Payment is not blocked");

        IsBlocked = false;
        BlockReason = null;
        Status = VerificationStatus.Verified;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentFraudPreventionUnblockedEvent(Id, PaymentId));
    }

    // Bu method risk score'u günceller, ama isBlocked ve status'u güncellemez (handler'da yapılmalı)
    public void UpdateRiskScore(int riskScore)
    {
        Guard.AgainstOutOfRange(riskScore, 0, 100, nameof(riskScore));
        _riskScore = riskScore;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentFraudPreventionUpdatedEvent(Id, PaymentId, CheckType, riskScore, Status));
    }

    public void UpdateStatus(VerificationStatus status)
    {
        if (Status == status)
            return;

        Status = status;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentFraudPreventionUpdatedEvent(Id, PaymentId, CheckType, _riskScore, status));
    }
}

