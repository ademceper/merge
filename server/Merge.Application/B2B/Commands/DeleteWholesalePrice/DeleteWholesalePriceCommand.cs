using MediatR;

namespace Merge.Application.B2B.Commands.DeleteWholesalePrice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record DeleteWholesalePriceCommand(Guid Id) : IRequest<bool>;

