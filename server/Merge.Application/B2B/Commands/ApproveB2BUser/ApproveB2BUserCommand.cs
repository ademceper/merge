using MediatR;

namespace Merge.Application.B2B.Commands.ApproveB2BUser;

public record ApproveB2BUserCommand(
    Guid Id,
    Guid ApprovedByUserId
) : IRequest<bool>;

