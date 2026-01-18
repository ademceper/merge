using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.CreateVolumeDiscount;

public record CreateVolumeDiscountCommand(CreateVolumeDiscountDto Dto) : IRequest<VolumeDiscountDto>;

