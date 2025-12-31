using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Interfaces.Logistics;

public interface IPickPackService
{
    Task<PickPackDto> CreatePickPackAsync(CreatePickPackDto dto);
    Task<PickPackDto?> GetPickPackByIdAsync(Guid id);
    Task<PickPackDto?> GetPickPackByPackNumberAsync(string packNumber);
    Task<IEnumerable<PickPackDto>> GetPickPacksByOrderIdAsync(Guid orderId);
    Task<IEnumerable<PickPackDto>> GetAllPickPacksAsync(string? status = null, Guid? warehouseId = null, int page = 1, int pageSize = 20);
    Task<bool> UpdatePickPackStatusAsync(Guid id, UpdatePickPackStatusDto dto, Guid? userId = null);
    Task<bool> StartPickingAsync(Guid id, Guid userId);
    Task<bool> CompletePickingAsync(Guid id, Guid userId);
    Task<bool> StartPackingAsync(Guid id, Guid userId);
    Task<bool> CompletePackingAsync(Guid id, Guid userId);
    Task<bool> MarkAsShippedAsync(Guid id);
    Task<bool> UpdatePickPackItemStatusAsync(Guid itemId, PickPackItemStatusDto dto);
    Task<Dictionary<string, int>> GetPickPackStatsAsync(Guid? warehouseId = null, DateTime? startDate = null, DateTime? endDate = null);
}

