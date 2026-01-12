using MediatR;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Commands.DeleteAddress;

public record DeleteAddressCommand(
    Guid Id,
    Guid? UserId = null,
    bool IsAdminOrManager = false
) : IRequest<bool>;
