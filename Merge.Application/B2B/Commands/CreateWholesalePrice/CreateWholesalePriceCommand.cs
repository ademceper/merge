using MediatR;
using Merge.Application.DTOs.B2B;

namespace Merge.Application.B2B.Commands.CreateWholesalePrice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateWholesalePriceCommand(CreateWholesalePriceDto Dto) : IRequest<WholesalePriceDto>;

