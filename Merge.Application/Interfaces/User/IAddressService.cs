using Merge.Application.DTOs.User;

namespace Merge.Application.Interfaces.User;

public interface IAddressService
{
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    Task<AddressDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AddressDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AddressDto> CreateAsync(CreateAddressDto dto, CancellationToken cancellationToken = default);
    Task<AddressDto> UpdateAsync(Guid id, UpdateAddressDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SetDefaultAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
}

