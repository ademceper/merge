using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Analytics;

public record TopCustomerDto(
    Guid CustomerId,
    string CustomerName,
    string Email,
    int OrderCount,
    decimal TotalSpent,
    DateTime LastOrderDate
);
