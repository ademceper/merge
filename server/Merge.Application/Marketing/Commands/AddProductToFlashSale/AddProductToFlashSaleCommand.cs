using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.AddProductToFlashSale;

public record AddProductToFlashSaleCommand(
    Guid FlashSaleId,
    Guid ProductId,
    decimal SalePrice,
    int StockLimit,
    int SortOrder) : IRequest<bool>;
