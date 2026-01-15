using MediatR;

namespace Merge.Application.LiveCommerce.Commands.JoinStream;

public record JoinStreamCommand(
    Guid StreamId,
    Guid? UserId,
    string? GuestId) : IRequest<Unit>;
