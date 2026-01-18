using MediatR;

namespace Merge.Application.Seller.Commands.CompletePayout;

public record CompletePayoutCommand(
    Guid PayoutId
) : IRequest<bool>;
