using MediatR;

namespace Merge.Application.Marketing.Commands.RemoveProductFromFlashSale;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RemoveProductFromFlashSaleCommand(
    Guid FlashSaleId,
    Guid ProductId) : IRequest<bool>;
