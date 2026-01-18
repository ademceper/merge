using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.UpdateVolumeDiscount;

public record UpdateVolumeDiscountCommand(
    Guid Id,
    CreateVolumeDiscountDto Dto
) : IRequest<bool>;

