using Merge.Domain.Exceptions;
using Merge.Domain.Common;
using Merge.Domain.Common.DomainEvents;

namespace Merge.Domain.Entities;

/// <summary>
/// PaymentMethod Entity - BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class PaymentMethod : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string IconUrl { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public bool RequiresOnlinePayment { get; private set; } = false;
    public bool RequiresManualVerification { get; private set; } = false;
    public decimal? MinimumAmount { get; private set; }
    public decimal? MaximumAmount { get; private set; }
    public decimal? ProcessingFee { get; private set; }
    public decimal? ProcessingFeePercentage { get; private set; }
    public string? Settings { get; private set; }
    public int DisplayOrder { get; private set; } = 0;
    public bool IsDefault { get; private set; } = false;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private PaymentMethod() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static PaymentMethod Create(
        string name,
        string code,
        string description,
        string iconUrl,
        bool isActive = true,
        bool requiresOnlinePayment = false,
        bool requiresManualVerification = false,
        decimal? minimumAmount = null,
        decimal? maximumAmount = null,
        decimal? processingFee = null,
        decimal? processingFeePercentage = null,
        string? settings = null,
        int displayOrder = 0,
        bool isDefault = false)
    {
        Guard.AgainstNullOrEmpty(name, nameof(name));
        Guard.AgainstNullOrEmpty(code, nameof(code));
        
        if (minimumAmount.HasValue && minimumAmount.Value < 0)
            throw new DomainException("Minimum tutar negatif olamaz");
        
        if (maximumAmount.HasValue && maximumAmount.Value < 0)
            throw new DomainException("Maksimum tutar negatif olamaz");
        
        if (minimumAmount.HasValue && maximumAmount.HasValue && minimumAmount.Value > maximumAmount.Value)
            throw new DomainException("Minimum tutar maksimum tutardan büyük olamaz");
        
        if (processingFee.HasValue && processingFee.Value < 0)
            throw new DomainException("İşlem ücreti negatif olamaz");
        
        if (processingFeePercentage.HasValue && (processingFeePercentage.Value < 0 || processingFeePercentage.Value > 100))
            throw new DomainException("İşlem ücreti yüzdesi 0-100 arasında olmalıdır");

        var paymentMethod = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            Description = description,
            IconUrl = iconUrl,
            IsActive = isActive,
            RequiresOnlinePayment = requiresOnlinePayment,
            RequiresManualVerification = requiresManualVerification,
            MinimumAmount = minimumAmount,
            MaximumAmount = maximumAmount,
            ProcessingFee = processingFee,
            ProcessingFeePercentage = processingFeePercentage,
            Settings = settings,
            DisplayOrder = displayOrder,
            IsDefault = isDefault,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ BOLUM 1.5: Domain Events - PaymentMethodCreatedEvent yayınla
        paymentMethod.AddDomainEvent(new PaymentMethodCreatedEvent(
            paymentMethod.Id,
            name,
            code,
            isActive,
            isDefault));

        return paymentMethod;
    }

    // ✅ BOLUM 1.1: Domain Logic - Update method
    public void Update(
        string? name = null,
        string? description = null,
        string? iconUrl = null,
        bool? isActive = null,
        bool? requiresOnlinePayment = null,
        bool? requiresManualVerification = null,
        decimal? minimumAmount = null,
        decimal? maximumAmount = null,
        decimal? processingFee = null,
        decimal? processingFeePercentage = null,
        string? settings = null,
        int? displayOrder = null)
    {
        if (!string.IsNullOrEmpty(name))
            Name = name;

        if (description != null)
            Description = description;

        if (iconUrl != null)
            IconUrl = iconUrl;

        if (isActive.HasValue)
            IsActive = isActive.Value;

        if (requiresOnlinePayment.HasValue)
            RequiresOnlinePayment = requiresOnlinePayment.Value;

        if (requiresManualVerification.HasValue)
            RequiresManualVerification = requiresManualVerification.Value;

        if (minimumAmount.HasValue)
        {
            if (minimumAmount.Value < 0)
                throw new DomainException("Minimum tutar negatif olamaz");
            MinimumAmount = minimumAmount;
        }

        if (maximumAmount.HasValue)
        {
            if (maximumAmount.Value < 0)
                throw new DomainException("Maksimum tutar negatif olamaz");
            MaximumAmount = maximumAmount;
        }

        if (minimumAmount.HasValue && maximumAmount.HasValue && minimumAmount.Value > maximumAmount.Value)
            throw new DomainException("Minimum tutar maksimum tutardan büyük olamaz");

        if (processingFee.HasValue)
        {
            if (processingFee.Value < 0)
                throw new DomainException("İşlem ücreti negatif olamaz");
            ProcessingFee = processingFee;
        }

        if (processingFeePercentage.HasValue)
        {
            if (processingFeePercentage.Value < 0 || processingFeePercentage.Value > 100)
                throw new DomainException("İşlem ücreti yüzdesi 0-100 arasında olmalıdır");
            ProcessingFeePercentage = processingFeePercentage;
        }

        if (settings != null)
            Settings = settings;

        if (displayOrder.HasValue)
            DisplayOrder = displayOrder.Value;

        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - PaymentMethodUpdatedEvent yayınla
        AddDomainEvent(new PaymentMethodUpdatedEvent(Id, Name, Code));
    }

    // ✅ BOLUM 1.1: Domain Logic - Activate/Deactivate
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - PaymentMethodActivatedEvent yayınla
        AddDomainEvent(new PaymentMethodActivatedEvent(Id, Name, Code));
    }

    public void Deactivate()
    {
        IsActive = false;
        IsDefault = false; // Deactivated methods cannot be default
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - PaymentMethodDeactivatedEvent yayınla
        AddDomainEvent(new PaymentMethodDeactivatedEvent(Id, Name, Code));
    }

    // ✅ BOLUM 1.1: Domain Logic - Set as default
    public void SetAsDefault()
    {
        if (!IsActive)
            throw new DomainException("Aktif olmayan ödeme yöntemi varsayılan olarak ayarlanamaz");
        
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;

        // ✅ BOLUM 1.5: Domain Events - PaymentMethodSetDefaultEvent yayınla
        AddDomainEvent(new PaymentMethodSetDefaultEvent(Id, Name, Code));
    }

    public void UnsetAsDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ BOLUM 1.1: Domain Logic - Calculate processing fee
    public decimal CalculateProcessingFee(decimal amount)
    {
        if (!IsActive)
            throw new DomainException("Aktif olmayan ödeme yöntemi için ücret hesaplanamaz");

        if (MinimumAmount.HasValue && amount < MinimumAmount.Value)
            throw new DomainException($"Tutar minimum tutardan ({MinimumAmount.Value}) küçük olamaz");

        if (MaximumAmount.HasValue && amount > MaximumAmount.Value)
            throw new DomainException($"Tutar maksimum tutardan ({MaximumAmount.Value}) büyük olamaz");

        var fee = ProcessingFee ?? 0;
        if (ProcessingFeePercentage.HasValue)
        {
            fee += amount * (ProcessingFeePercentage.Value / 100);
        }

        return Math.Round(fee, 2);
    }

    // ✅ BOLUM 1.1: Domain Logic - Check if amount is valid
    public bool IsAmountValid(decimal amount)
    {
        if (!IsActive)
            return false;

        if (MinimumAmount.HasValue && amount < MinimumAmount.Value)
            return false;

        if (MaximumAmount.HasValue && amount > MaximumAmount.Value)
            return false;

        return true;
    }
}

