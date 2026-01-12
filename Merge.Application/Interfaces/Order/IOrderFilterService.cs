using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Order;

public interface IOrderFilterService
{
    Task<IEnumerable<OrderDto>> GetFilteredOrdersAsync(OrderFilterDto filter, CancellationToken cancellationToken = default);
    Task<OrderStatisticsDto> GetOrderStatisticsAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

