using Merge.Domain.Modules.Ordering;
namespace Merge.Application.DTOs.Cart;

/// <summary>
/// Pre Order Stats DTO - BOLUM 7.1.5: Records (ZORUNLU)
/// </summary>
public record PreOrderStatsDto(
    int TotalPreOrders,
    int PendingPreOrders,
    int ConfirmedPreOrders,
    decimal TotalRevenue,
    decimal TotalDeposits,
    IReadOnlyList<PreOrderDto> RecentPreOrders
);
