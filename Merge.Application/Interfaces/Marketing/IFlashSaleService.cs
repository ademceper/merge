using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Marketing;

public interface IFlashSaleService
{
    Task<FlashSaleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<FlashSaleDto>> GetActiveSalesAsync(CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    Task<PagedResult<FlashSaleDto>> GetActiveSalesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IEnumerable<FlashSaleDto>> GetAllAsync(CancellationToken cancellationToken = default);
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    Task<PagedResult<FlashSaleDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<FlashSaleDto> CreateAsync(CreateFlashSaleDto dto, CancellationToken cancellationToken = default);
    Task<FlashSaleDto> UpdateAsync(Guid id, UpdateFlashSaleDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> AddProductToSaleAsync(Guid flashSaleId, AddProductToSaleDto dto, CancellationToken cancellationToken = default);
    Task<bool> RemoveProductFromSaleAsync(Guid flashSaleId, Guid productId, CancellationToken cancellationToken = default);
}

