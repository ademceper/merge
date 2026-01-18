using MediatR;

namespace Merge.Application.Marketing.Commands.RemoveProductFromFlashSale;

public record RemoveProductFromFlashSaleCommand(
    Guid FlashSaleId,
    Guid ProductId) : IRequest<bool>;
