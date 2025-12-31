namespace Merge.Domain.Entities;

public class B2BUser : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid OrganizationId { get; set; }
    public string? EmployeeId { get; set; } // Company employee ID
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public string Status { get; set; } = "Active"; // Active, Inactive, Suspended
    public bool IsApproved { get; set; } = false;
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public decimal? CreditLimit { get; set; } // Credit limit for this user
    public decimal? UsedCredit { get; set; } = 0; // Currently used credit
    public string? Settings { get; set; } // JSON for B2B-specific settings
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Organization Organization { get; set; } = null!;
    public User? ApprovedBy { get; set; }
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
}

public class WholesalePrice : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid? OrganizationId { get; set; } // Organization-specific pricing
    public int MinQuantity { get; set; } // Minimum quantity for this price tier
    public int? MaxQuantity { get; set; } // Maximum quantity (null = unlimited)
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Navigation properties
    public Product Product { get; set; } = null!;
    public Organization? Organization { get; set; }
}

public class CreditTerm : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty; // e.g., "Net 30", "Net 60"
    public int PaymentDays { get; set; } // Number of days to pay
    public decimal? CreditLimit { get; set; } // Maximum credit limit
    public decimal? UsedCredit { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public string? Terms { get; set; } // Additional terms description
    
    // Navigation properties
    public Organization Organization { get; set; } = null!;
}

public class PurchaseOrder : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Guid? B2BUserId { get; set; } // User who created the PO
    public string PONumber { get; set; } = string.Empty; // Auto-generated: PO-XXXXXX
    public string Status { get; set; } = "Draft"; // Draft, Submitted, Approved, Rejected, Fulfilled, Cancelled
    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public Guid? CreditTermId { get; set; }
    
    // Navigation properties
    public Organization Organization { get; set; } = null!;
    public B2BUser? B2BUser { get; set; }
    public User? ApprovedBy { get; set; }
    public CreditTerm? CreditTerm { get; set; }
    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}

public class PurchaseOrderItem : BaseEntity
{
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
}

public class VolumeDiscount : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid? CategoryId { get; set; } // Category-wide discount
    public Guid? OrganizationId { get; set; } // Organization-specific discount
    public int MinQuantity { get; set; }
    public int? MaxQuantity { get; set; }
    public decimal DiscountPercentage { get; set; } // Percentage discount
    public decimal? FixedDiscountAmount { get; set; } // Fixed amount discount (alternative to percentage)
    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Navigation properties
    public Product? Product { get; set; }
    public Category? Category { get; set; }
    public Organization? Organization { get; set; }
}

