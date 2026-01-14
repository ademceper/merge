using Merge.Domain.Modules.Ordering;
namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Abandoned Cart Recovery Stats DTO - BOLUM 7.1.5: Records (ZORUNLU)
/// </summary>
public record AbandonedCartRecoveryStatsDto(
    int TotalAbandonedCarts,
    decimal TotalAbandonedValue,
    int EmailsSent,
    int EmailsOpened,
    int EmailsClicked,
    int RecoveredCarts,
    decimal RecoveredRevenue,
    decimal RecoveryRate,
    decimal AverageCartValue
);
