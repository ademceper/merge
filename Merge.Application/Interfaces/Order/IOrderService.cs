using Merge.Application.DTOs.Order;

namespace Merge.Application.Interfaces.Order;

public interface IOrderService
{
    Task<OrderDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(Guid userId);
    Task<OrderDto> CreateOrderFromCartAsync(Guid userId, Guid addressId, string? couponCode = null);
    Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, string status);
    Task<bool> CancelOrderAsync(Guid orderId);
    Task<OrderDto> ReorderAsync(Guid orderId, Guid userId);
    Task<byte[]> ExportOrdersToCsvAsync(OrderExportDto exportDto);
    Task<byte[]> ExportOrdersToJsonAsync(OrderExportDto exportDto);
    Task<byte[]> ExportOrdersToExcelAsync(OrderExportDto exportDto);
}

