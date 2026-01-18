namespace Merge.Application.DTOs.Seller;

public record CommissionTierDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal MinSales { get; init; }
    public decimal MaxSales { get; init; }
    public decimal CommissionRate { get; init; }
    public decimal PlatformFeeRate { get; init; }
    public bool IsActive { get; init; }
    public int Priority { get; init; }
}
