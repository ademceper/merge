using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;

namespace Merge.Application.Interfaces.Logistics;

public interface IStockMovementService
{
    Task<StockMovementDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockMovementDto>> GetByInventoryIdAsync(Guid inventoryId, CancellationToken cancellationToken = default);
    Task<PagedResult<StockMovementDto>> GetByProductIdAsync(Guid productId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<PagedResult<StockMovementDto>> GetByWarehouseIdAsync(Guid warehouseId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<StockMovementDto>> GetFilteredAsync(StockMovementFilterDto filter, CancellationToken cancellationToken = default);
    Task<StockMovementDto> CreateAsync(CreateStockMovementDto createDto, Guid userId, CancellationToken cancellationToken = default);
}
