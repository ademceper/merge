namespace Merge.Application.DTOs.Seller;

public class SellerPerformanceMetricsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    // Sales Metrics
    public decimal TotalSales { get; set; }
    public decimal PreviousPeriodSales { get; set; }
    public decimal SalesGrowth { get; set; } // Percentage
    public int TotalOrders { get; set; }
    public int PreviousPeriodOrders { get; set; }
    public decimal OrderGrowth { get; set; } // Percentage
    public decimal OrdersGrowth { get; set; } // Percentage (alias for OrderGrowth)
    public decimal AverageOrderValue { get; set; }
    public decimal PreviousPeriodAOV { get; set; }
    
    // Customer Metrics
    public int TotalCustomers { get; set; }
    public int UniqueCustomers { get; set; }
    public int NewCustomers { get; set; }
    public int ReturningCustomers { get; set; }
    public decimal CustomerRetentionRate { get; set; } // Percentage
    public decimal CustomerLifetimeValue { get; set; }
    
    // Product Metrics
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public decimal AverageProductRating { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    
    // Order Fulfillment Metrics
    public decimal AverageFulfillmentTime { get; set; } // Hours
    public decimal AverageShippingTime { get; set; } // Hours
    public int OnTimeDeliveryRate { get; set; } // Percentage
    public int LateDeliveries { get; set; }
    
    // Return & Refund Metrics
    public int TotalReturns { get; set; }
    public decimal ReturnRate { get; set; } // Percentage
    public decimal TotalRefunds { get; set; }
    public decimal RefundRate { get; set; } // Percentage
    
    // Conversion Metrics
    public int ProductViews { get; set; }
    public int AddToCarts { get; set; }
    public decimal ConversionRate { get; set; } // Percentage
    public decimal CartAbandonmentRate { get; set; } // Percentage
    
    // Category Performance
    public List<CategoryPerformanceDto> CategoryPerformance { get; set; } = new();
    
    // Time-based Trends
    public List<SalesTrendDto> SalesTrends { get; set; } = new();
    public List<OrderTrendDto> OrderTrends { get; set; } = new();
    
    // Top/Bottom Performers
    public List<SellerTopProductDto> TopProducts { get; set; } = new();
    public List<SellerTopProductDto> WorstProducts { get; set; } = new();
}
