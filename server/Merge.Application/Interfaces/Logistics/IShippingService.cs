using Merge.Application.DTOs.Logistics;
using Merge.Domain.Enums;

namespace Merge.Application.Interfaces.Logistics;

public interface IShippingService
{
    Task<ShippingDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ShippingDto?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<ShippingDto> CreateShippingAsync(CreateShippingDto dto, CancellationToken cancellationToken = default);
    Task<ShippingDto> UpdateTrackingAsync(Guid shippingId, string trackingNumber, CancellationToken cancellationToken = default);
    Task<ShippingDto> UpdateStatusAsync(Guid shippingId, ShippingStatus status, CancellationToken cancellationToken = default);
    Task<decimal> CalculateShippingCostAsync(Guid orderId, string shippingProvider, CancellationToken cancellationToken = default);
    Task<IEnumerable<ShippingProviderDto>> GetAvailableProvidersAsync(CancellationToken cancellationToken = default);
}

