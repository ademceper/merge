using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Interfaces.User;

public interface IAddressService
{
    Task<AddressDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AddressDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AddressDto> CreateAsync(CreateAddressDto dto, CancellationToken cancellationToken = default);
    Task<AddressDto> UpdateAsync(Guid id, UpdateAddressDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SetDefaultAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}

