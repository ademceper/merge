using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Interfaces.Logistics;

public interface IWarehouseService
{
    Task<WarehouseDto?> GetByIdAsync(Guid id);
    Task<WarehouseDto?> GetByCodeAsync(string code);
    Task<IEnumerable<WarehouseDto>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<WarehouseDto>> GetActiveWarehousesAsync();
    Task<WarehouseDto> CreateAsync(CreateWarehouseDto createDto);
    Task<WarehouseDto> UpdateAsync(Guid id, UpdateWarehouseDto updateDto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ActivateAsync(Guid id);
    Task<bool> DeactivateAsync(Guid id);
}
