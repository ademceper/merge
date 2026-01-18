using Merge.Domain.Modules.Ordering;
namespace Merge.Application.DTOs.Cart;


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
