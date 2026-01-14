using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketing;
namespace Merge.Domain.Enums;

/// <summary>
/// Referral Status - BOLUM 1.0: Entity Dosya Organizasyonu (ZORUNLU)
/// Her enum dosyasında SADECE 1 enum olmalı
/// </summary>
public enum ReferralStatus
{
    Pending, // User signed up but hasn't made purchase
    Completed, // User made first purchase
    Rewarded, // Referrer was rewarded
    Expired // Referral expired before completion
}

