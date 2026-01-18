using MediatR;

namespace Merge.Application.Seller.Commands.FailPayout;

public record FailPayoutCommand(
    Guid PayoutId,
    string Reason
) : IRequest<bool>;
