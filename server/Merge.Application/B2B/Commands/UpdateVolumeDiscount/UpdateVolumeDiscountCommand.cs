using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.UpdateVolumeDiscount;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateVolumeDiscountCommand(
    Guid Id,
    CreateVolumeDiscountDto Dto
) : IRequest<bool>;

