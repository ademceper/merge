using MediatR;

namespace Merge.Application.B2B.Commands.DeleteVolumeDiscount;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteVolumeDiscountCommand(Guid Id) : IRequest<bool>;

