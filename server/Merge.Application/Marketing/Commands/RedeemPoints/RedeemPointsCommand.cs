using MediatR;

namespace Merge.Application.Marketing.Commands.RedeemPoints;

public record RedeemPointsCommand(
    Guid UserId,
    int Points,
    Guid? OrderId) : IRequest<bool>;
