using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.CreateFlashSale;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateFlashSaleCommand(
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string? BannerImageUrl) : IRequest<FlashSaleDto>;
