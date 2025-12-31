namespace Merge.Application.DTOs.Cart;

public class AbandonedCartConfigDto
{
    public int AbandonmentThresholdHours { get; set; } = 1;
    public int FirstEmailDelayHours { get; set; } = 2;
    public int SecondEmailDelayHours { get; set; } = 24;
    public int FinalEmailDelayHours { get; set; } = 72;
    public bool AutoSendEmails { get; set; } = true;
    public decimal DefaultCouponDiscount { get; set; } = 10;
}
