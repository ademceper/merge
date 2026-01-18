using MediatR;

namespace Merge.Application.Seller.Commands.ApproveSellerApplication;

public record ApproveSellerApplicationCommand(
    Guid ApplicationId,
    Guid ReviewerId
) : IRequest<bool>;
