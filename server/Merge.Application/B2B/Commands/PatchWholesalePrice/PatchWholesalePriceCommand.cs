using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.PatchWholesalePrice;

public record PatchWholesalePriceCommand(
    Guid Id,
    PatchWholesalePriceDto PatchDto
) : IRequest<bool>;
