using MediatR;

namespace Merge.Application.B2B.Commands.DeleteB2BUser;

public record DeleteB2BUserCommand(Guid Id) : IRequest<bool>;

