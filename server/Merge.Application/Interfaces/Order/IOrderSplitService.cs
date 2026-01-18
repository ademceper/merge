using Merge.Application.DTOs.Order;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Interfaces.Order;

public interface IOrderSplitService
{
    Task<OrderSplitDto> SplitOrderAsync(Guid orderId, CreateOrderSplitDto dto, CancellationToken cancellationToken = default);
    Task<OrderSplitDto?> GetSplitAsync(Guid splitId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderSplitDto>> GetOrderSplitsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderSplitDto>> GetSplitOrdersAsync(Guid splitOrderId, CancellationToken cancellationToken = default);
    Task<bool> CancelSplitAsync(Guid splitId, CancellationToken cancellationToken = default);
    Task<bool> CompleteSplitAsync(Guid splitId, CancellationToken cancellationToken = default);
}

