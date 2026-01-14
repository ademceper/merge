using MediatR;

namespace Merge.Application.Seller.Commands.RejectSellerApplication;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RejectSellerApplicationCommand(
    Guid ApplicationId,
    string Reason,
    Guid ReviewerId
) : IRequest<bool>;
