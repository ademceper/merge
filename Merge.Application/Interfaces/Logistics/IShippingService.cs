using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Interfaces.Logistics;

public interface IShippingService
{
    Task<ShippingDto?> GetByIdAsync(Guid id);
    Task<ShippingDto?> GetByOrderIdAsync(Guid orderId);
    Task<ShippingDto> CreateShippingAsync(CreateShippingDto dto);
    Task<ShippingDto> UpdateTrackingAsync(Guid shippingId, string trackingNumber);
    Task<ShippingDto> UpdateStatusAsync(Guid shippingId, string status);
    Task<decimal> CalculateShippingCostAsync(Guid orderId, string shippingProvider);
    Task<IEnumerable<ShippingProviderDto>> GetAvailableProvidersAsync();
}

