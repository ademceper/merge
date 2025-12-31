using Merge.Application.DTOs.Order;

namespace Merge.Application.Interfaces.Order;

public interface IOrderFilterService
{
    Task<IEnumerable<OrderDto>> GetFilteredOrdersAsync(OrderFilterDto filter);
    Task<OrderStatisticsDto> GetOrderStatisticsAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
}

