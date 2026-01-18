using MediatR;

namespace Merge.Application.Marketing.Commands.AddPoints;

public record AddPointsCommand(
    Guid UserId,
    int Points,
    string Type,
    string Description,
    Guid? OrderId = null) : IRequest<bool>;
