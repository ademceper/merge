using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Interfaces.Catalog;

public interface IInventoryService
{
    Task<InventoryDto?> GetByIdAsync(Guid id);
    Task<InventoryDto?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId);
    Task<IEnumerable<InventoryDto>> GetByProductIdAsync(Guid productId);
    Task<IEnumerable<InventoryDto>> GetByWarehouseIdAsync(Guid warehouseId);
    Task<IEnumerable<LowStockAlertDto>> GetLowStockAlertsAsync(Guid? warehouseId = null);
    Task<StockReportDto?> GetStockReportByProductAsync(Guid productId);
    Task<InventoryDto> CreateAsync(CreateInventoryDto createDto);
    Task<InventoryDto> UpdateAsync(Guid id, UpdateInventoryDto updateDto);
    Task<bool> DeleteAsync(Guid id);
    Task<InventoryDto> AdjustStockAsync(AdjustInventoryDto adjustDto, Guid userId);
    Task<bool> TransferStockAsync(TransferInventoryDto transferDto, Guid userId);
    Task<bool> ReserveStockAsync(Guid productId, Guid warehouseId, int quantity, Guid? orderId = null);
    Task<bool> ReleaseStockAsync(Guid productId, Guid warehouseId, int quantity, Guid? orderId = null);
    Task<int> GetAvailableStockAsync(Guid productId, Guid? warehouseId = null);
    Task<bool> UpdateLastCountDateAsync(Guid inventoryId);
}
