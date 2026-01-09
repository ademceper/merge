using MediatR;

namespace Merge.Application.LiveCommerce.Commands.JoinStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record JoinStreamCommand(
    Guid StreamId,
    Guid? UserId,
    string? GuestId) : IRequest<Unit>;

