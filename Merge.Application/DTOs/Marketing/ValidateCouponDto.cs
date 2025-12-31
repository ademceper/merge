namespace Merge.Application.DTOs.Marketing;

public class ValidateCouponDto
{
    public string Code { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
    public Guid? UserId { get; set; }
    public List<Guid>? ProductIds { get; set; }
}
