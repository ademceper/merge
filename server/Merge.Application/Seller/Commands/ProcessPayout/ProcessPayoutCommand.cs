using MediatR;

namespace Merge.Application.Seller.Commands.ProcessPayout;

public record ProcessPayoutCommand(
    Guid PayoutId,
    string TransactionReference
) : IRequest<bool>;
