using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;

namespace Merge.Application.Interfaces.Catalog;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
public interface IInventoryService
{
    Task<InventoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InventoryDto?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventoryDto>> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    Task<PagedResult<InventoryDto>> GetByWarehouseIdAsync(Guid warehouseId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination - PagedResult dönmeli (ZORUNLU)
    Task<PagedResult<LowStockAlertDto>> GetLowStockAlertsAsync(Guid? warehouseId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<StockReportDto?> GetStockReportByProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<InventoryDto> CreateAsync(CreateInventoryDto createDto, CancellationToken cancellationToken = default);
    Task<InventoryDto> UpdateAsync(Guid id, UpdateInventoryDto updateDto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<InventoryDto> AdjustStockAsync(AdjustInventoryDto adjustDto, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> TransferStockAsync(TransferInventoryDto transferDto, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ReserveStockAsync(Guid productId, Guid warehouseId, int quantity, Guid? orderId = null, CancellationToken cancellationToken = default);
    Task<bool> ReleaseStockAsync(Guid productId, Guid warehouseId, int quantity, Guid? orderId = null, CancellationToken cancellationToken = default);
    Task<int> GetAvailableStockAsync(Guid productId, Guid? warehouseId = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateLastCountDateAsync(Guid inventoryId, CancellationToken cancellationToken = default);
}
