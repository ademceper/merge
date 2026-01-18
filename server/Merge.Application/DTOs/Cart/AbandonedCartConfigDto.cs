using Merge.Domain.Modules.Ordering;
namespace Merge.Application.DTOs.Cart;


public record AbandonedCartConfigDto(
    int AbandonmentThresholdHours,
    int FirstEmailDelayHours,
    int SecondEmailDelayHours,
    int FinalEmailDelayHours,
    bool AutoSendEmails,
    decimal DefaultCouponDiscount
);
