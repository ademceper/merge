using Merge.Application.DTOs.Order;

namespace Merge.Application.Interfaces.Order;

public interface IOrderSplitService
{
    Task<OrderSplitDto> SplitOrderAsync(Guid orderId, CreateOrderSplitDto dto);
    Task<OrderSplitDto?> GetSplitAsync(Guid splitId);
    Task<IEnumerable<OrderSplitDto>> GetOrderSplitsAsync(Guid orderId);
    Task<IEnumerable<OrderSplitDto>> GetSplitOrdersAsync(Guid splitOrderId);
    Task<bool> CancelSplitAsync(Guid splitId);
    Task<bool> CompleteSplitAsync(Guid splitId);
}

