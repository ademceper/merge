using Merge.Application.DTOs.Logistics;
using Merge.Application.Common;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Logistics;

public interface IPickPackService
{
    Task<PickPackDto> CreatePickPackAsync(CreatePickPackDto dto, CancellationToken cancellationToken = default);
    Task<PickPackDto?> GetPickPackByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PickPackDto?> GetPickPackByPackNumberAsync(string packNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<PickPackDto>> GetPickPacksByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    Task<PagedResult<PickPackDto>> GetAllPickPacksAsync(string? status = null, Guid? warehouseId = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> UpdatePickPackStatusAsync(Guid id, UpdatePickPackStatusDto dto, Guid? userId = null, CancellationToken cancellationToken = default);
    Task<bool> StartPickingAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CompletePickingAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> StartPackingAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> CompletePackingAsync(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> MarkAsShippedAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> UpdatePickPackItemStatusAsync(Guid itemId, PickPackItemStatusDto dto, CancellationToken cancellationToken = default);
    // ⚠️ NOTE: Dictionary<string, int> burada kabul edilebilir çünkü stats için key-value çiftleri dinamik
    Task<Dictionary<string, int>> GetPickPackStatsAsync(Guid? warehouseId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
}

