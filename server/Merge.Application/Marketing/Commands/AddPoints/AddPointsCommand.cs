using MediatR;

namespace Merge.Application.Marketing.Commands.AddPoints;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record AddPointsCommand(
    Guid UserId,
    int Points,
    string Type,
    string Description,
    Guid? OrderId = null) : IRequest<bool>;
