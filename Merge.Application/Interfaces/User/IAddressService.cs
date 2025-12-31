using Merge.Application.DTOs.User;

namespace Merge.Application.Interfaces.User;

public interface IAddressService
{
    Task<AddressDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<AddressDto>> GetByUserIdAsync(Guid userId);
    Task<AddressDto> CreateAsync(CreateAddressDto dto);
    Task<AddressDto> UpdateAsync(Guid id, UpdateAddressDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> SetDefaultAsync(Guid id, Guid userId);
}

