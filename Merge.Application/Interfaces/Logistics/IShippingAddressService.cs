using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Interfaces.Logistics;

public interface IShippingAddressService
{
    Task<ShippingAddressDto> CreateShippingAddressAsync(Guid userId, CreateShippingAddressDto dto);
    Task<ShippingAddressDto?> GetShippingAddressByIdAsync(Guid id);
    Task<IEnumerable<ShippingAddressDto>> GetUserShippingAddressesAsync(Guid userId, bool? isActive = null);
    Task<ShippingAddressDto?> GetDefaultShippingAddressAsync(Guid userId);
    Task<bool> UpdateShippingAddressAsync(Guid id, UpdateShippingAddressDto dto);
    Task<bool> DeleteShippingAddressAsync(Guid id);
    Task<bool> SetDefaultShippingAddressAsync(Guid userId, Guid addressId);
}

