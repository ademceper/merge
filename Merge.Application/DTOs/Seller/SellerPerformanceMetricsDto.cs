namespace Merge.Application.DTOs.Seller;

// ✅ BOLUM 7.1.5: Records - DTO'lar record olarak tanımlanmalı (ZORUNLU)
// ✅ BOLUM 8.0: Over-posting Protection - init-only properties (ZORUNLU)
public record SellerPerformanceMetricsDto
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    
    // Sales Metrics
    public decimal TotalSales { get; init; }
    public decimal PreviousPeriodSales { get; init; }
    public decimal SalesGrowth { get; init; } // Percentage
    public int TotalOrders { get; init; }
    public int PreviousPeriodOrders { get; init; }
    public decimal OrderGrowth { get; init; } // Percentage
    public decimal OrdersGrowth { get; init; } // Percentage (alias for OrderGrowth)
    public decimal AverageOrderValue { get; init; }
    public decimal PreviousPeriodAOV { get; init; }
    
    // Customer Metrics
    public int TotalCustomers { get; init; }
    public int UniqueCustomers { get; init; }
    public int NewCustomers { get; init; }
    public int ReturningCustomers { get; init; }
    public decimal CustomerRetentionRate { get; init; } // Percentage
    public decimal CustomerLifetimeValue { get; init; }
    
    // Product Metrics
    public int TotalProducts { get; init; }
    public int ActiveProducts { get; init; }
    public int LowStockProducts { get; init; }
    public int OutOfStockProducts { get; init; }
    public decimal AverageProductRating { get; init; }
    public decimal AverageRating { get; init; }
    public int TotalReviews { get; init; }
    
    // Order Fulfillment Metrics
    public decimal AverageFulfillmentTime { get; init; } // Hours
    public decimal AverageShippingTime { get; init; } // Hours
    public int OnTimeDeliveryRate { get; init; } // Percentage
    public int LateDeliveries { get; init; }
    
    // Return & Refund Metrics
    public int TotalReturns { get; init; }
    public decimal ReturnRate { get; init; } // Percentage
    public decimal TotalRefunds { get; init; }
    public decimal RefundRate { get; init; } // Percentage
    
    // Conversion Metrics
    public int ProductViews { get; init; }
    public int AddToCarts { get; init; }
    public decimal ConversionRate { get; init; } // Percentage
    public decimal CartAbandonmentRate { get; init; } // Percentage
    
    // Category Performance
    public List<CategoryPerformanceDto> CategoryPerformance { get; init; } = new();
    
    // Time-based Trends
    public List<SalesTrendDto> SalesTrends { get; init; } = new();
    public List<OrderTrendDto> OrderTrends { get; init; } = new();
    
    // Top/Bottom Performers
    public List<SellerTopProductDto> TopProducts { get; init; } = new();
    public List<SellerTopProductDto> WorstProducts { get; init; } = new();
}
