using MediatR;
using Merge.Application.DTOs.Marketing;

namespace Merge.Application.Marketing.Commands.CreateFlashSale;

public record CreateFlashSaleCommand(
    string Title,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string? BannerImageUrl) : IRequest<FlashSaleDto>;
