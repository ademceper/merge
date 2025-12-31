using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Interfaces.Logistics;

public interface IStockMovementService
{
    Task<StockMovementDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<StockMovementDto>> GetByInventoryIdAsync(Guid inventoryId);
    Task<IEnumerable<StockMovementDto>> GetByProductIdAsync(Guid productId, int page = 1, int pageSize = 20);
    Task<IEnumerable<StockMovementDto>> GetByWarehouseIdAsync(Guid warehouseId, int page = 1, int pageSize = 20);
    Task<IEnumerable<StockMovementDto>> GetFilteredAsync(StockMovementFilterDto filter);
    Task<StockMovementDto> CreateAsync(CreateStockMovementDto createDto, Guid userId);
}
