using Merge.Domain.Modules.Ordering;
namespace Merge.Application.DTOs.Cart;


public record PreOrderStatsDto(
    int TotalPreOrders,
    int PendingPreOrders,
    int ConfirmedPreOrders,
    decimal TotalRevenue,
    decimal TotalDeposits,
    IReadOnlyList<PreOrderDto> RecentPreOrders
);
