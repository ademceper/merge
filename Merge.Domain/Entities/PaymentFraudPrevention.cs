using Merge.Domain.Enums;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;
using Merge.Domain.Exceptions;

namespace Merge.Domain.Entities;

/// <summary>
/// PaymentFraudPrevention Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class PaymentFraudPrevention : BaseEntity, IAggregateRoot
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid PaymentId { get; private set; }
    
    // ✅ BOLUM 1.2: Enum kullanımı (string CheckType YASAK)
    public PaymentCheckType CheckType { get; private set; }
    
    // ✅ BOLUM 1.2: Enum kullanımı (string Status yerine) - BEST_PRACTICES_ANALIZI.md BOLUM 1.1.6
    public VerificationStatus Status { get; private set; } = VerificationStatus.Pending;
    
    public bool IsBlocked { get; private set; } = false;
    public string? BlockReason { get; private set; }
    
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
    
    public string? CheckResult { get; private set; } // JSON for detailed results
    public DateTime? CheckedAt { get; private set; }
    public string? DeviceFingerprint { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    
    // Navigation properties
    public Payment Payment { get; set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private PaymentFraudPrevention() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    // ✅ BOLUM 12.0: Magic number'lar handler'da belirlenir, Domain entity configuration'a bağımlı değil
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

        // ✅ BOLUM 1.5: Domain Events
        check.AddDomainEvent(new PaymentFraudPreventionCreatedEvent(
            check.Id,
            paymentId,
            checkType,
            riskScore,
            status));

        return check;
    }

    // ✅ BOLUM 1.1: Domain Method - Block payment
    public void Block(string reason)
    {
        Guard.AgainstNullOrEmpty(reason, nameof(reason));

        if (IsBlocked)
            throw new DomainException("Payment is already blocked");

        IsBlocked = true;
        BlockReason = reason;
        Status = VerificationStatus.Failed;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new PaymentFraudPreventionBlockedEvent(Id, PaymentId, reason));
    }

    // ✅ BOLUM 1.1: Domain Method - Unblock payment
    public void Unblock()
    {
        if (!IsBlocked)
            throw new DomainException("Payment is not blocked");

        IsBlocked = false;
        BlockReason = null;
        Status = VerificationStatus.Verified;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events
        AddDomainEvent(new PaymentFraudPreventionUnblockedEvent(Id, PaymentId));
    }

    // ✅ BOLUM 1.1: Domain Method - Update risk score
    // ✅ BOLUM 12.0: Magic number'lar handler'da belirlenir, Domain entity configuration'a bağımlı değil
    // Bu method risk score'u günceller, ama isBlocked ve status'u güncellemez (handler'da yapılmalı)
    public void UpdateRiskScore(int riskScore)
    {
        Guard.AgainstOutOfRange(riskScore, 0, 100, nameof(riskScore));
        _riskScore = riskScore;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Method - Update status
    public void UpdateStatus(VerificationStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}

