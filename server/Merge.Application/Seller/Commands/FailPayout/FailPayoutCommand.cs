using MediatR;

namespace Merge.Application.Seller.Commands.FailPayout;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record FailPayoutCommand(
    Guid PayoutId,
    string Reason
) : IRequest<bool>;
