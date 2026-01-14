using MediatR;

namespace Merge.Application.Marketing.Commands.RedeemPoints;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record RedeemPointsCommand(
    Guid UserId,
    int Points,
    Guid? OrderId) : IRequest<bool>;
