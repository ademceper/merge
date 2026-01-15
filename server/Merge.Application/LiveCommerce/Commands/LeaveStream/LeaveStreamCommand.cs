using MediatR;

namespace Merge.Application.LiveCommerce.Commands.LeaveStream;

public record LeaveStreamCommand(
    Guid StreamId,
    Guid? UserId,
    string? GuestId) : IRequest<Unit>;
