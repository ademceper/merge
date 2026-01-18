namespace Merge.Application.DTOs.Analytics;

public record CustomerSegmentDto(
    string Segment, // New, Active, Inactive, VIP
    int CustomerCount,
    decimal TotalRevenue,
    decimal AverageOrderValue
);
