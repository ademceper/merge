using Merge.Application.Common;
using Merge.Application.DTOs.Order;
using Merge.Domain.Enums;

namespace Merge.Application.Interfaces.Order;

public interface IOrderService
{
    Task<OrderDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderDto>> GetOrdersByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<OrderDto> CreateOrderFromCartAsync(Guid userId, Guid addressId, string? couponCode = null, CancellationToken cancellationToken = default);
    // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
    Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus status, CancellationToken cancellationToken = default);
    Task<bool> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<OrderDto> ReorderAsync(Guid orderId, Guid userId, CancellationToken cancellationToken = default);
    Task<byte[]> ExportOrdersToCsvAsync(OrderExportDto exportDto, CancellationToken cancellationToken = default);
    Task<byte[]> ExportOrdersToJsonAsync(OrderExportDto exportDto, CancellationToken cancellationToken = default);
    Task<byte[]> ExportOrdersToExcelAsync(OrderExportDto exportDto, CancellationToken cancellationToken = default);
}

