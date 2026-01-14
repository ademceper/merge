using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.CreateVolumeDiscount;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateVolumeDiscountCommand(CreateVolumeDiscountDto Dto) : IRequest<VolumeDiscountDto>;

