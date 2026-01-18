using MediatR;

namespace Merge.Application.Seller.Commands.RejectSellerApplication;

public record RejectSellerApplicationCommand(
    Guid ApplicationId,
    string Reason,
    Guid ReviewerId
) : IRequest<bool>;
