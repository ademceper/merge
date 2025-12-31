using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Interfaces.Marketing;

public interface IGiftCardService
{
    Task<GiftCardDto?> GetByCodeAsync(string code);
    Task<GiftCardDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<GiftCardDto>> GetUserGiftCardsAsync(Guid userId);
    Task<GiftCardDto> PurchaseGiftCardAsync(Guid userId, PurchaseGiftCardDto dto);
    Task<GiftCardDto> RedeemGiftCardAsync(string code, Guid userId);
    Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount);
    Task<bool> ApplyGiftCardToOrderAsync(string code, Guid orderId, Guid userId);
}

