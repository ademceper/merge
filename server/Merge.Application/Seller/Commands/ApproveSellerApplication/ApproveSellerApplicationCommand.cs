using MediatR;

namespace Merge.Application.Seller.Commands.ApproveSellerApplication;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ApproveSellerApplicationCommand(
    Guid ApplicationId,
    Guid ReviewerId
) : IRequest<bool>;
