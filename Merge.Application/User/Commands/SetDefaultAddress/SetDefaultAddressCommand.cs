using MediatR;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.User.Commands.SetDefaultAddress;

public record SetDefaultAddressCommand(
    Guid Id,
    Guid UserId
) : IRequest<bool>;
