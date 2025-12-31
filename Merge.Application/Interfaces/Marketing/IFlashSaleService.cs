using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Interfaces.Marketing;

public interface IFlashSaleService
{
    Task<FlashSaleDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<FlashSaleDto>> GetActiveSalesAsync();
    Task<IEnumerable<FlashSaleDto>> GetAllAsync();
    Task<FlashSaleDto> CreateAsync(CreateFlashSaleDto dto);
    Task<FlashSaleDto> UpdateAsync(Guid id, UpdateFlashSaleDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> AddProductToSaleAsync(Guid flashSaleId, AddProductToSaleDto dto);
    Task<bool> RemoveProductFromSaleAsync(Guid flashSaleId, Guid productId);
}

