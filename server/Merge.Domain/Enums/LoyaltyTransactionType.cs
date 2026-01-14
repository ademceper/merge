using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
namespace Merge.Domain.Enums;

/// <summary>
/// Loyalty Transaction Type - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum LoyaltyTransactionType
{
    Purchase, // Points earned from purchase
    Review, // Points for writing review
    Referral, // Points for referring friend
    Signup, // Welcome bonus
    Birthday, // Birthday bonus
    Redeem, // Points redeemed for discount/reward
    Expired, // Points expired
    Adjustment, // Manual adjustment by admin
    Bonus, // Special promotion bonus
    Return // Points deducted on return
}

