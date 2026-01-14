using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.AddProductToFlashSale;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AddProductToFlashSaleCommand(
    Guid FlashSaleId,
    Guid ProductId,
    decimal SalePrice,
    int StockLimit,
    int SortOrder) : IRequest<bool>;
