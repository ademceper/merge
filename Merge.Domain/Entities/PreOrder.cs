namespace Merge.Domain.Entities;

public class PreOrder : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public int Quantity { get; set; } = 1;
    public decimal Price { get; set; }
    public decimal DepositAmount { get; set; } = 0; // Partial payment required
    public decimal DepositPaid { get; set; } = 0;
    public PreOrderStatus Status { get; set; } = PreOrderStatus.Pending;
    public DateTime ExpectedAvailabilityDate { get; set; }
    public DateTime? ActualAvailabilityDate { get; set; }
    public DateTime? NotificationSentAt { get; set; }
    public DateTime? ConvertedToOrderAt { get; set; }
    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? Notes { get; set; }
    public string? VariantOptions { get; set; } // JSON for selected variant options
}

public class PreOrderCampaign : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public int MaxQuantity { get; set; } = 0; // 0 = unlimited
    public int CurrentQuantity { get; set; } = 0;
    public decimal DepositPercentage { get; set; } = 0; // Percentage of price required as deposit
    public decimal SpecialPrice { get; set; } = 0; // Special pre-order price
    public bool IsActive { get; set; } = true;
    public bool NotifyOnAvailable { get; set; } = true;
    public ICollection<PreOrder> PreOrders { get; set; } = new List<PreOrder>();
}

public enum PreOrderStatus
{
    Pending,
    DepositPaid,
    Confirmed,
    ReadyToShip,
    Converted,
    Cancelled,
    Expired
}
