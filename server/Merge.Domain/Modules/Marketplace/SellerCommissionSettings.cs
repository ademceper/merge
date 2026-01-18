using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;
using Merge.Domain.Exceptions;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Payment;

namespace Merge.Domain.Modules.Marketplace;

/// <summary>
/// SellerCommissionSettings Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class SellerCommissionSettings : BaseEntity
{
    public Guid SellerId { get; private set; }
    public decimal CustomCommissionRate { get; private set; } = 0; // Override default tier rate
    public bool UseCustomRate { get; private set; } = false;
    public decimal MinimumPayoutAmount { get; private set; } = 100; // Minimum amount to request payout
    public string? PaymentMethod { get; private set; } // Bank transfer, PayPal, etc.
    public string? PaymentDetails { get; private set; } // Account number, PayPal email, etc.

    // Navigation properties
    public User Seller { get; private set; } = null!;

    private SellerCommissionSettings() { }

    public static SellerCommissionSettings Create(
        Guid sellerId,
        decimal minimumPayoutAmount = 100)
    {
        Guard.AgainstDefault(sellerId, nameof(sellerId));
        Guard.AgainstNegative(minimumPayoutAmount, nameof(minimumPayoutAmount));

        return new SellerCommissionSettings
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            CustomCommissionRate = 0,
            UseCustomRate = false,
            MinimumPayoutAmount = minimumPayoutAmount,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void UpdateCustomCommissionRate(decimal commissionRate, bool useCustomRate)
    {
        Guard.AgainstNegative(commissionRate, nameof(commissionRate));

        if (useCustomRate && commissionRate > 100)
            throw new DomainException("Komisyon oranı %100'den fazla olamaz");

        CustomCommissionRate = commissionRate;
        UseCustomRate = useCustomRate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMinimumPayoutAmount(decimal minimumPayoutAmount)
    {
        Guard.AgainstNegativeOrZero(minimumPayoutAmount, nameof(minimumPayoutAmount));

        MinimumPayoutAmount = minimumPayoutAmount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePaymentMethod(string? paymentMethod, string? paymentDetails)
    {
        if (string.IsNullOrWhiteSpace(paymentMethod) && !string.IsNullOrWhiteSpace(paymentDetails))
            throw new DomainException("Ödeme detayları belirtilmişse ödeme yöntemi de belirtilmelidir");

        PaymentMethod = paymentMethod;
        PaymentDetails = paymentDetails;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanRequestPayout(decimal availableBalance)
    {
        return availableBalance >= MinimumPayoutAmount;
    }
}

