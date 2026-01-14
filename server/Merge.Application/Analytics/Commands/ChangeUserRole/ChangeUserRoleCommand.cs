using MediatR;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Analytics.Commands.ChangeUserRole;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ChangeUserRoleCommand(
    Guid UserId,
    string Role
) : IRequest<bool>;

