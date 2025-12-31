namespace Merge.Application.DTOs.Analytics;

public class CustomerAnalyticsDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalCustomers { get; set; }
    public int NewCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int ReturningCustomers { get; set; }
    public decimal AverageLifetimeValue { get; set; }
    public decimal AveragePurchaseFrequency { get; set; }
    public List<CustomerSegmentDto> CustomerSegments { get; set; } = new();
    public List<TopCustomerDto> TopCustomers { get; set; } = new();
    public List<TimeSeriesDataPoint> CustomerAcquisition { get; set; } = new();
}
