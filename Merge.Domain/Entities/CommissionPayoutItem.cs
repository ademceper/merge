using Merge.Domain.Common;

namespace Merge.Domain.Entities;

/// <summary>
/// CommissionPayoutItem Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class CommissionPayoutItem : BaseEntity
{
    // ✅ BOLUM 1.1: Rich Domain Model - Private setters for encapsulation
    public Guid PayoutId { get; private set; }
    public Guid CommissionId { get; private set; }

    // Navigation properties
    public CommissionPayout Payout { get; private set; } = null!;
    public SellerCommission Commission { get; private set; } = null!;

    // ✅ BOLUM 1.1: Factory Method - Private constructor
    private CommissionPayoutItem() { }

    // ✅ BOLUM 1.1: Factory Method with validation
    public static CommissionPayoutItem Create(
        Guid payoutId,
        Guid commissionId)
    {
        Guard.AgainstDefault(payoutId, nameof(payoutId));
        Guard.AgainstDefault(commissionId, nameof(commissionId));

        return new CommissionPayoutItem
        {
            Id = Guid.NewGuid(),
            PayoutId = payoutId,
            CommissionId = commissionId,
            CreatedAt = DateTime.UtcNow
        };
    }
}

