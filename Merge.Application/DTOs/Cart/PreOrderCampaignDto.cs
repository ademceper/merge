namespace Merge.Application.DTOs.Cart;

public class PreOrderCampaignDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImage { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public int MaxQuantity { get; set; }
    public int CurrentQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public decimal DepositPercentage { get; set; }
    public decimal SpecialPrice { get; set; }
    public bool IsActive { get; set; }
    public bool IsFull { get; set; }
}
