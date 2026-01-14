using MediatR;

namespace Merge.Application.Seller.Commands.ProcessPayout;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ProcessPayoutCommand(
    Guid PayoutId,
    string TransactionReference
) : IRequest<bool>;
