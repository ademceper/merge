using Merge.Application.DTOs.Marketing;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
namespace Merge.Application.Interfaces.Marketing;

public interface IGiftCardService
{
    Task<GiftCardDto?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<GiftCardDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<GiftCardDto>> GetUserGiftCardsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<GiftCardDto> PurchaseGiftCardAsync(Guid userId, PurchaseGiftCardDto dto, CancellationToken cancellationToken = default);
    Task<GiftCardDto> RedeemGiftCardAsync(string code, Guid userId, CancellationToken cancellationToken = default);
    Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount, CancellationToken cancellationToken = default);
    Task<bool> ApplyGiftCardToOrderAsync(string code, Guid orderId, Guid userId, CancellationToken cancellationToken = default);
}

