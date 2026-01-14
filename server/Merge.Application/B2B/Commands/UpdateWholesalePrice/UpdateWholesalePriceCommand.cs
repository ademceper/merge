using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.UpdateWholesalePrice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateWholesalePriceCommand(
    Guid Id,
    CreateWholesalePriceDto Dto
) : IRequest<bool>;

