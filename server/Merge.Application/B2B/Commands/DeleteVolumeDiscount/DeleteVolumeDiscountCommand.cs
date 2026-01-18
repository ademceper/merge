using MediatR;

namespace Merge.Application.B2B.Commands.DeleteVolumeDiscount;

public record DeleteVolumeDiscountCommand(Guid Id) : IRequest<bool>;

