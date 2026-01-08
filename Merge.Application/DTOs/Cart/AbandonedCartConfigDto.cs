namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Abandoned Cart Config DTO - BOLUM 7.1.5: Records (ZORUNLU)
/// </summary>
public record AbandonedCartConfigDto(
    int AbandonmentThresholdHours,
    int FirstEmailDelayHours,
    int SecondEmailDelayHours,
    int FinalEmailDelayHours,
    bool AutoSendEmails,
    decimal DefaultCouponDiscount
);
