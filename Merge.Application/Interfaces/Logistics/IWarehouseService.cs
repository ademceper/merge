using Merge.Application.DTOs.Logistics;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Logistics;

public interface IWarehouseService
{
    Task<WarehouseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WarehouseDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<WarehouseDto>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IEnumerable<WarehouseDto>> GetActiveWarehousesAsync(CancellationToken cancellationToken = default);
    Task<WarehouseDto> CreateAsync(CreateWarehouseDto createDto, CancellationToken cancellationToken = default);
    Task<WarehouseDto> UpdateAsync(Guid id, UpdateWarehouseDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
