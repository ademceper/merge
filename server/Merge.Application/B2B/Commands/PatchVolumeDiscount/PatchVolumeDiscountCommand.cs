using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.PatchVolumeDiscount;

public record PatchVolumeDiscountCommand(
    Guid Id,
    PatchVolumeDiscountDto PatchDto
) : IRequest<bool>;
