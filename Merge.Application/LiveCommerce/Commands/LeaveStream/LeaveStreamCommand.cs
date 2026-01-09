using MediatR;

namespace Merge.Application.LiveCommerce.Commands.LeaveStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record LeaveStreamCommand(
    Guid StreamId,
    Guid? UserId,
    string? GuestId) : IRequest<Unit>;

