using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.UpdateWholesalePrice;

public record UpdateWholesalePriceCommand(
    Guid Id,
    CreateWholesalePriceDto Dto
) : IRequest<bool>;

