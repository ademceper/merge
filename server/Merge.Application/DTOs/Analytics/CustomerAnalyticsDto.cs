namespace Merge.Application.DTOs.Analytics;

// ✅ BOLUM 7.1: Records kullanımı (immutable DTOs) (ZORUNLU)
public record CustomerAnalyticsDto(
    DateTime StartDate,
    DateTime EndDate,
    int TotalCustomers,
    int NewCustomers,
    int ActiveCustomers,
    int ReturningCustomers,
    decimal AverageLifetimeValue,
    decimal AveragePurchaseFrequency,
    List<CustomerSegmentDto> CustomerSegments,
    List<TopCustomerDto> TopCustomers,
    List<TimeSeriesDataPoint> CustomerAcquisition
);
