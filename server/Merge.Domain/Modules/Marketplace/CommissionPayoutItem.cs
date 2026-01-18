using Merge.Domain.SharedKernel;
using Merge.Domain.SharedKernel;

namespace Merge.Domain.Modules.Marketplace;

/// <summary>
/// CommissionPayoutItem Entity - Rich Domain Model implementation
/// BOLUM 1.1: Rich Domain Model (ZORUNLU)
/// </summary>
public class CommissionPayoutItem : BaseEntity
{
    public Guid PayoutId { get; private set; }
    public Guid CommissionId { get; private set; }

    // Navigation properties
    public CommissionPayout Payout { get; private set; } = null!;
    public SellerCommission Commission { get; private set; } = null!;

    private CommissionPayoutItem() { }

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

