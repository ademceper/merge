using MediatR;

namespace Merge.Application.Seller.Commands.CompletePayout;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CompletePayoutCommand(
    Guid PayoutId
) : IRequest<bool>;
