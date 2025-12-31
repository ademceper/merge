namespace Merge.Application.DTOs.Seller;

public class CommissionTierDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MinSales { get; set; }
    public decimal MaxSales { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal PlatformFeeRate { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
}
