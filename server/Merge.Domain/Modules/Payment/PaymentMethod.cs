using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel.DomainEvents;
using Merge.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Merge.Domain.Modules.Payment;

/// <summary>
/// PaymentMethod Entity - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// BOLUM 1.4: Aggregate Root Pattern (ZORUNLU) - Domain event'ler için IAggregateRoot implement edilmeli
/// BOLUM 1.5: Domain Events (ZORUNLU)
/// BOLUM 1.7: Concurrency Control (ZORUNLU)
/// Her entity dosyasında SADECE 1 class olmalı
/// </summary>
public class PaymentMethod : BaseEntity, IAggregateRoot
{
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

    [Timestamp]
    public byte[]? RowVersion { get; set; }

    private PaymentMethod() { }

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

        paymentMethod.AddDomainEvent(new PaymentMethodCreatedEvent(
            paymentMethod.Id,
            name,
            code,
            isActive,
            isDefault));

        return paymentMethod;
    }

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

        if (description is not null)
            Description = description;

        if (iconUrl is not null)
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

        if (settings is not null)
            Settings = settings;

        if (displayOrder.HasValue)
            DisplayOrder = displayOrder.Value;

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentMethodUpdatedEvent(Id, Name, Code));
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentMethodActivatedEvent(Id, Name, Code));
    }

    public void Deactivate()
    {
        IsActive = false;
        IsDefault = false; // Deactivated methods cannot be default
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentMethodDeactivatedEvent(Id, Name, Code));
    }

    public void SetAsDefault()
    {
        if (!IsActive)
            throw new DomainException("Aktif olmayan ödeme yöntemi varsayılan olarak ayarlanamaz");
        
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentMethodSetDefaultEvent(Id, Name, Code));
    }

    public void UnsetAsDefault()
    {
        if (!IsDefault)
            return;

        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentMethodUnsetDefaultEvent(Id, Name, Code));
    }

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

    public void MarkAsDeleted()
    {
        if (IsDeleted)
            return;

        if (IsDefault)
            throw new DomainException("Varsayılan ödeme yöntemi silinemez. Önce varsayılan durumunu kaldırın.");

        IsDeleted = true;
        IsActive = false;
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PaymentMethodDeletedEvent(Id, Name, Code));
    }
}

