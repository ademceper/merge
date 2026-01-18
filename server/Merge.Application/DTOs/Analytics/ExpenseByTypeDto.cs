using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
namespace Merge.Application.DTOs.Analytics;

public record ExpenseByTypeDto(
    string ExpenseType, // Shipping, Commission, Refund, Discount, etc.
    decimal Amount,
    decimal Percentage
);
