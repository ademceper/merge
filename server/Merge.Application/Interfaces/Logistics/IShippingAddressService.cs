using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Interfaces.Logistics;

public interface IShippingAddressService
{
    Task<ShippingAddressDto> CreateShippingAddressAsync(Guid userId, CreateShippingAddressDto dto, CancellationToken cancellationToken = default);
    Task<ShippingAddressDto?> GetShippingAddressByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ShippingAddressDto>> GetUserShippingAddressesAsync(Guid userId, bool? isActive = null, CancellationToken cancellationToken = default);
    Task<ShippingAddressDto?> GetDefaultShippingAddressAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateShippingAddressAsync(Guid id, UpdateShippingAddressDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteShippingAddressAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SetDefaultShippingAddressAsync(Guid userId, Guid addressId, CancellationToken cancellationToken = default);
}

