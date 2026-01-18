using MediatR;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Analytics.Commands.ChangeUserRole;

public record ChangeUserRoleCommand(
    Guid UserId,
    string Role
) : IRequest<bool>;

