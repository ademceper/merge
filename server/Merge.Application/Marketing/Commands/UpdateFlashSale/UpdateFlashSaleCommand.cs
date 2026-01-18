using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.UpdateFlashSale;

public record UpdateFlashSaleCommand(
    Guid Id,
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    string? BannerImageUrl) : IRequest<FlashSaleDto>;
